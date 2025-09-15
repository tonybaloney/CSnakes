using CSnakes.Runtime.Python;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

/// <summary>
/// Bridges .NET logging to Python logging.
/// Create a Python log handler that stores logs in memory and emits them back to .NET.
/// </summary>
/// <param name="logger"></param>
public static class PythonLogger
{
    // TODO : Handle Close request via a destructor or IDisposable pattern.
    // It should call the handlers' .close() method

    public static void WithPythonLogging(this IPythonEnvironment env, ILogger logger)
    {
        using var handler = CreateHandler(env);
        RecordListener(handler, logger);
    }

    internal static PyObject CreateHandler(IPythonEnvironment env, string? logger = null)
    {
        using PyObject loggingHandlersModule = Import.ImportModule("logging.handlers");
        using PyObject memoryHandlerClass = loggingHandlersModule.GetAttr("MemoryHandler");
        string handlerPythonCode = @"
import logging
import time

class __csnakesMemoryHandler(logging.Handler):
    def __init__(self):
        logging.Handler.__init__(self)
        self.records = []
        self.closeRequest = False

    def emit(self, record):
        self.records.append(record)

    def get_records(self):  # equivalent to flush()
        while not self.closeRequest:
            with self.lock:
                for record in self.records:
                    yield record
                self.records.clear()
            time.sleep(0.01) # TODO: Find a better solution for the generator loop

    def close(self):
        self.closeRequest = True
        logging.Handler.close(self)

def installCSnakesHandler(handler, name = None):
    logging.getLogger(name).addHandler(handler)

";
        using var module = Import.ImportModule("_csnakesLoggingBridge", handlerPythonCode, "_csnakesLoggingBridge.py");
        using var __csnakesMemoryHandlerCls = module.GetAttr("__csnakesMemoryHandler");
        var handler = __csnakesMemoryHandlerCls.Call();
        using var __installCSnakesHandler = module.GetAttr("installCSnakesHandler");
        __installCSnakesHandler.Call(handler, PyObject.From(logger));
        return handler;
    }

    private static void HandleRecord(ILogger logger, int level, string message)
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
        IGeneratorIterator<PyObject, PyObject, PyObject> generator;
        using (GIL.Acquire())
        {
            using PyObject getRecordsMethod = handler.GetAttr("get_records");
            using PyObject __result_pyObject = getRecordsMethod.Call();
            generator = __result_pyObject.BareImportAs<IGeneratorIterator<PyObject, PyObject, PyObject>, PyObjectImporters.Generator<PyObject, PyObject, PyObject, PyObjectImporters.Runtime<PyObject>, PyObjectImporters.Runtime<PyObject>>>();
        }

        var callback = new Action<PyObject>(
            // This is the callback that will be called from the generator
            record => {
                using (GIL.Acquire())
                {
                    using PyObject level = generator.Current.GetAttr("levelno");
                    using PyObject message = generator.Current.GetAttr("msg");
                    HandleRecord(logger, level.As<int>(), message.As<string>());
                }
            }
        );

        // Wait for the generator to finish
        var task = Task.Run(() =>
        {
            while (generator.MoveNext())
            {
                callback(generator.Current);
            }
        });
    }
}
