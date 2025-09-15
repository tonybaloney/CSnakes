using CSnakes.Runtime.Python;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

/// <summary>
/// Bridges .NET logging to Python logging.
/// Create a Python log handler that stores logs in memory and emits them back to .NET.
/// </summary>
public static class PythonLogger
{
    // TODO : Handle Close request via a destructor or IDisposable pattern.
    // It should call the handlers' .close() method

    public static void WithPythonLogging(this IPythonEnvironment env, ILogger logger)
    {
        using var handler = CreateHandler();
        RecordListener(handler, logger);
    }

    internal static PyObject CreateHandler(string? loggerName = null)
    {
        const string handlerPythonCode = """
            import logging
            import queue
            import time


            class __csnakesMemoryHandler(logging.Handler):
                def __init__(self):
                    logging.Handler.__init__(self)
                    self.queue = queue.Queue()

                def emit(self, record):
                    try:
                        self.queue.put(record)
                    except queue.ShutDown:
                        pass

                def get_records(self):  # equivalent to flush()
                    while True:
                        try:
                            record = self.queue.get()
                        except queue.ShutDown:
                            break
                        else:
                            self.queue.task_done()
                            yield (record.levelno, record.msg)

                def close(self):
                    self.queue.shutdown()
                    logging.Handler.close(self)

            def installCSnakesHandler(handler, name = None):
                logging.getLogger(name).addHandler(handler)

            """;

        using (GIL.Acquire())
        {
            using var module = Import.ImportModule("_csnakesLoggingBridge", handlerPythonCode, "_csnakesLoggingBridge.py");
            using var __csnakesMemoryHandlerCls = module.GetAttr("__csnakesMemoryHandler");
            var handler = __csnakesMemoryHandlerCls.Call();
            using var __installCSnakesHandler = module.GetAttr("installCSnakesHandler");
            __installCSnakesHandler.Call(handler, PyObject.From(loggerName));

            return handler;
        }
    }

    private static void HandleRecord(ILogger logger, long level, string message)
    {
        // TODO: Handle Attributes and other useful things.
        // Look at the LogRecord class for details.
        switch (level)
        {
            // https://docs.python.org/3/library/logging.html#levels
            case >= 50:
                logger.LogCritical(message);
                break;
            case >= 40:
                logger.LogError(message);
                break;
            case >= 30:
                logger.LogWarning(message);
                break;
            case >= 20:
                logger.LogInformation(message);
                break;
            case >= 10:
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
        var task = Task.Run(() =>
        {
            while (generator.MoveNext())
            {
                var (level, message) = generator.Current;
                HandleRecord(logger, (int)level, message);
            }
        });
    }
}
