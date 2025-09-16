using CSnakes.Runtime.Python;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ExceptionInfo = (CSnakes.Runtime.Python.PyObject ExceptionType, CSnakes.Runtime.Python.PyObject Exception, CSnakes.Runtime.Python.PyObject Traceback);

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

    private sealed class Bridge(PyObject handler, PyObject uninstallCSnakesHandler, Task task) : IDisposable
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
            using var handlerClass = module.GetAttr("__csnakesMemoryHandler");
            using var installCSnakesHandler = module.GetAttr("installCSnakesHandler");

            Task task;

            try
            {
                using (GIL.Acquire())
                {
                    handler = handlerClass.Call();
                    uninstallCSnakesHandler = module.GetAttr("uninstallCSnakesHandler");

                    using var loggerNameStr = PyObject.From(loggerName);
                    installCSnakesHandler.Call(handler, loggerNameStr).Dispose();
                }
                task = RecordListener(handler, logger);
            }
            catch
            {
                handler?.Dispose();
                uninstallCSnakesHandler?.Dispose();
                throw;
            }

            return new(handler, uninstallCSnakesHandler, task);
        }

        private static void HandleRecord(ILogger logger, int level, string message, Exception? exception)
        {
            // https://docs.python.org/3/library/logging.html#levels
            var mappedLevel = (level / 10) switch
            {
                1 => LogLevel.Debug,
                2 => LogLevel.Information,
                3 => LogLevel.Warning,
                4 => LogLevel.Error,
                >= 5 => LogLevel.Critical,
                _ => LogLevel.None,
            };

            if (mappedLevel == LogLevel.None || !logger.IsEnabled(mappedLevel))
                return;

            logger.Log(mappedLevel, exception, message);
        }

        private static Task RecordListener(PyObject handler, ILogger logger)
        {
            IGeneratorIterator<(long, string, ExceptionInfo?), PyObject, PyObject> generator;
            using (GIL.Acquire())
            {
                using PyObject getRecordsMethod = handler.GetAttr("get_records");
                using PyObject getRecordsResult = getRecordsMethod.Call();
                generator =
                    getRecordsResult.BareImportAs<IGeneratorIterator<(long, string, ExceptionInfo?), PyObject, PyObject>,
                                                                     PyObjectImporters.Generator<(long, string, ExceptionInfo?), PyObject, PyObject,
                                                                                                 PyObjectImporters.Tuple<long, string, ExceptionInfo?,
                                                                                                                         PyObjectImporters.Int64,
                                                                                                                         PyObjectImporters.String,
                                                                                                                         NoneValueImporter<ExceptionInfo,
                                                                                                                                           PyObjectImporters.Tuple<PyObject, PyObject, PyObject,
                                                                                                                                                                   PyObjectImporters.Runtime<PyObject>,
                                                                                                                                                                   PyObjectImporters.Runtime<PyObject>,
                                                                                                                                                                   PyObjectImporters.Runtime<PyObject>>>>,
                                                                                                 PyObjectImporters.Runtime<PyObject>>>();
            }

            return Task.Run(() =>
            {
                while (generator.MoveNext())
                {
                    var (level, message, exceptionInfo) = generator.Current;

                    Exception? exception = null;
                    if (exceptionInfo is { } info)
                    {
                        using var pyExceptionType = info.ExceptionType.NoneToNull();
                        using var pyException = info.Exception.NoneToNull();
                        using var traceback = info.Traceback.NoneToNull();
                        using var name = pyExceptionType?.GetAttr("__name__");
                        exception = new PythonInvocationException(name?.ToString() ?? "Exception", pyException, traceback, message);
                    }

                    HandleRecord(logger, (int)level, message, exception);
                }
            });
        }

        public void Dispose()
        {
            var uninstalled = false;

            try
            {
                uninstallCSnakesHandler.Call(handler).Dispose();
                uninstalled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during uninstallation of handler: {ex}");
            }

            if (uninstalled)
            {
                if (Task.WaitAny([task], TimeSpan.FromSeconds(5)) < 0)
                {
                    Debug.WriteLine("Timeout waiting for logging task to complete.");
                }
                else
                {
                    try
                    {
                        task.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in logging task: {ex}");
                    }
                }
            }

            handler.Dispose();
            uninstallCSnakesHandler.Dispose();
        }

        private sealed class NoneValueImporter<T, TImporter> : IPyObjectImporter<T?>
            where T : struct
            where TImporter : IPyObjectImporter<T>
        {
            private NoneValueImporter() { }

            static T? IPyObjectImporter<T?>.BareImport(PyObject obj) =>
                !obj.IsNone() ? TImporter.BareImport(obj) : null;
        }
    }
}
