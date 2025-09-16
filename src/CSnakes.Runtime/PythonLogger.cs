using CSnakes.Runtime.Python;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using ExceptionTuple = (CSnakes.Runtime.Python.PyObject? exceptionType, CSnakes.Runtime.Python.PyObject? exception, CSnakes.Runtime.Python.PyObject? traceback);

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

    internal static IDisposable EnableGlobalLogging(ILogger logger)
    {
        return Bridge.Create(logger);
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
                            yield (record.levelno, record.getMessage(), record.exc_info)

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

        private static void HandleRecord(ILogger logger, int level, string message, ExceptionTuple? exceptionInfo)
        {
            Exception? exception = null;
            if (exceptionInfo != null)
            {
                using var name = exceptionInfo.Value.exceptionType?.GetAttr("__name__") ?? null;
                exception = new PythonInvocationException(name?.ToString() ?? "Exception", exceptionInfo.Value.exception, exceptionInfo.Value.traceback, message);
            }

            // https://docs.python.org/3/library/logging.html#levels
            var mappedLevel = (level / 10) switch
            {
                1 => LogLevel.Debug,
                2 => LogLevel.Information,
                3 => LogLevel.Warning,
                4 => LogLevel.Error,
                5 => LogLevel.Critical,
                _ => LogLevel.None,
            };

            if (mappedLevel == LogLevel.None || !logger.IsEnabled(mappedLevel))
                return;

            logger.Log(mappedLevel, exception, message);
        }

        internal static void RecordListener(PyObject handler, ILogger logger)
        {
            IGeneratorIterator<(long, string, PyObject), PyObject, PyObject> generator;
            using (GIL.Acquire())
            {
                using PyObject getRecordsMethod = handler.GetAttr("get_records");
                using PyObject __result_pyObject = getRecordsMethod.Call();
                generator = __result_pyObject.BareImportAs<IGeneratorIterator<(long, string, PyObject), PyObject, PyObject>, PyObjectImporters.Generator<(long, string, PyObject), PyObject, PyObject, global::CSnakes.Runtime.Python.PyObjectImporters.Tuple<long, string, PyObject, global::CSnakes.Runtime.Python.PyObjectImporters.Int64, global::CSnakes.Runtime.Python.PyObjectImporters.String, PyObjectImporters.Runtime<PyObject>>, PyObjectImporters.Runtime<PyObject>>>();
            }

            // Wait for the generator to finish
            _ = Task.Run(() =>
            {
                while (generator.MoveNext())
                {
                    var (level, message, exception) = generator.Current;
                    if (exception.IsNone())
                    {
                        HandleRecord(logger, (int)level, message, null);
                    } else {
                        HandleRecord(logger, (int)level, message, exception.As<ExceptionTuple>());
                    }
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
