using CSnakes.Runtime.Python;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.Runtime;

/// <summary>
/// Bridges .NET logging to Python logging.
/// Create a Python log handler that stores logs in memory and emits them back to .NET.
/// </summary>
public static class PythonLogger
{
    public static IDisposable WithPythonLogging(this IPythonEnvironment env, ILogger logger, string? loggerName = null)
    {
        return Bridge.Create(logger, loggerName);
    }

    private sealed class Bridge(PyObject handler, PyObject uninstallCSnakesHandler) : IDisposable
    {
        internal static Bridge Create(ILogger logger, string? loggerName = null)
        {
            const string handlerPythonCode = """
                import logging
                import queue


                class __csnakesMemoryHandler(logging.Handler):
                    def __init__(self):
                        logging.Handler.__init__(self)
                        self.queue = queue.Queue()

                    def emit(self, record):
                        try:
                            self.queue.put(record)
                        except queue.ShutDown:
                            pass

                    def get_records(self):
                        while True:
                            record = self.queue.get()
                            self.queue.task_done()
                            if record is None:
                                break
                            yield (record.levelno, record.getMessage())

                    def close(self):
                        self.queue.put(None)
                        logging.Handler.close(self)


                def installCSnakesHandler(handler, name = None):
                    logging.getLogger(name).addHandler(handler)


                def uninstallCSnakesHandler(handler, name = None):
                    handler.close()
                    logging.getLogger(name).removeHandler(handler)

                """;

                
            PyObject? handler = null;
            PyObject? uninstallCSnakesHandler = null;

            using var module = Import.ImportModule("_csnakesLoggingBridge", handlerPythonCode, "_csnakesLoggingBridge.py");
            using var __csnakesMemoryHandlerCls = module.GetAttr("__csnakesMemoryHandler");
            using var __installCSnakesHandler = module.GetAttr("installCSnakesHandler");

            try
            {
                using (GIL.Acquire())
                {
                    handler = __csnakesMemoryHandlerCls.Call();
                    uninstallCSnakesHandler = module.GetAttr("uninstallCSnakesHandler");

                    using var loggerNameStr = PyObject.From(loggerName);
                    __installCSnakesHandler.Call(handler, loggerNameStr).Dispose();
                }
                RecordListener(handler, logger);
            }
            catch
            {
                handler?.Dispose();
                uninstallCSnakesHandler?.Dispose();
                throw;
            }

            return new(handler, uninstallCSnakesHandler);
        }

        private static void HandleRecord(ILogger logger, long level, string message)
        {
            // TODO: Handle Attributes and other useful things.
            // Look at the LogRecord class for details.
            switch (level)
            {
                // https://docs.python.org/3/library/logging.html#levels
                case >= 50 when logger.IsEnabled(LogLevel.Critical):
                    logger.LogCritical(message);
                    break;
                case >= 40 when logger.IsEnabled(LogLevel.Error):
                    logger.LogError(message);
                    break;
                case >= 30 when logger.IsEnabled(LogLevel.Warning):
                    logger.LogWarning(message);
                    break;
                case >= 20 when logger.IsEnabled(LogLevel.Information):
                    logger.LogInformation(message);
                    break;
                case >= 10 when logger.IsEnabled(LogLevel.Debug):
                    logger.LogDebug(message);
                    break;
                default:
                    // NOTSET
                    break;
            }
        }

        internal static void RecordListener(PyObject handler, ILogger logger)
        {
            IGeneratorIterator<(long, string), PyObject, PyObject> generator;
            using (GIL.Acquire())
            {
                using PyObject getRecordsMethod = handler.GetAttr("get_records");
                using PyObject __result_pyObject = getRecordsMethod.Call();
                generator = __result_pyObject.BareImportAs<IGeneratorIterator<(long, string), PyObject, PyObject>, PyObjectImporters.Generator<(long, string), PyObject, PyObject, PyObjectImporters.Tuple<long, string, PyObjectImporters.Int64, PyObjectImporters.String>, PyObjectImporters.Runtime<PyObject>>>();
            }

            // Wait for the generator to finish
            _ = Task.Run(() =>
            {
                while (generator.MoveNext())
                {
                    var (level, message) = generator.Current;
                    HandleRecord(logger, (int)level, message);
                }
            });
        }

        public void Dispose()
        {
            try
            {
                uninstallCSnakesHandler.Call(handler).Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during uninstallation of handler: {ex}");
            }

            handler.Dispose();
            uninstallCSnakesHandler.Dispose();
        }
    }
}
