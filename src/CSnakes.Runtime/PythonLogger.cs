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
    public static IAsyncDisposable WithPythonLogging(this IPythonEnvironment env, ILogger logger, string? loggerName = null) =>
        Bridge.Create(logger, loggerName);

    internal static IAsyncDisposable EnableGlobalLogging(ILogger logger) =>
        Bridge.Create(logger);
}

file sealed class Bridge(PyObject handler, PyObject uninstallCSnakesHandler, Task listenerTask) : IAsyncDisposable
{
    const string ModuleCode = """
        import logging
        import queue
        import threading


        class __csnakesMemoryHandler(logging.Handler):
            def __init__(self):
                logging.Handler.__init__(self)
                self.queue = queue.Queue(200)
                self.stats_lock = threading.Lock()
                self.drop_count = 0

            def emit(self, record):
                _ = self._put(record)

            def _put(self, record, attempts = 10):
                for _ in range(attempts):  # attempts to try enqueue the record
                    try:
                        self.queue.put_nowait(record)
                        return True  # successfully enqueued
                    except queue.Full:
                        try:
                            _ = self.queue.get_nowait()  # drop the oldest record
                            self.queue.task_done()
                            with self.stats_lock:
                                self.drop_count += 1
                        except queue.Empty:
                            pass
                return False  # all attempts to put failed

            def get_records(self):
                while True:
                    record = self.queue.get()
                    self.queue.task_done()
                    if record is None:
                        break
                    with self.stats_lock:
                        drop_count = self.drop_count
                        self.drop_count = 0
                    yield (drop_count, record.levelno, record.getMessage(), record.exc_info)

            def close(self):
                _ = self._put(None)
                logging.Handler.close(self)


        def installCSnakesHandler(handler, name = None):
            logging.getLogger(name).addHandler(handler)


        def uninstallCSnakesHandler(handler, name = None):
            logging.getLogger(name).removeHandler(handler)
            handler.close()

        """;

    private bool disposed;

    internal static Bridge Create(ILogger logger, string? loggerName = null)
    {
        PyObject? handler = null;
        PyObject? uninstallCSnakesHandler = null;

        using var module = Import.ImportModule("_csnakesLoggingBridge", ModuleCode, "_csnakesLoggingBridge.py");
        using var handlerClass = module.GetAttr("__csnakesMemoryHandler");
        using var installCSnakesHandler = module.GetAttr("installCSnakesHandler");

        Task listenerTask;

        try
        {
            using (GIL.Acquire())
            {
                handler = handlerClass.Call();
                uninstallCSnakesHandler = module.GetAttr("uninstallCSnakesHandler");

                using var loggerNameStr = PyObject.From(loggerName);
                installCSnakesHandler.Call(handler, loggerNameStr).Dispose();

                listenerTask = StartRecordListener(handler, logger);
            }
        }
        catch
        {
            handler?.Dispose();
            uninstallCSnakesHandler?.Dispose();
            throw;
        }

        return new(handler, uninstallCSnakesHandler, listenerTask);
    }

    private static Task StartRecordListener(PyObject handler, ILogger logger)
    {
        using PyObject getRecords = handler.GetAttr("get_records");
        using PyObject getRecordsResult = getRecords.Call();
        IGeneratorIterator<(long, long, string, ExceptionInfo?), PyObject, PyObject> generator =
            getRecordsResult.ImportAs<IGeneratorIterator<(long, long, string, ExceptionInfo?), PyObject, PyObject>,
                                                         PyObjectImporters.Generator<(long, long, string, ExceptionInfo?), PyObject, PyObject,
                                                                                     PyObjectImporters.Tuple<long, long, string, ExceptionInfo?,
                                                                                                             PyObjectImporters.Int64,
                                                                                                             PyObjectImporters.Int64,
                                                                                                             PyObjectImporters.String,
                                                                                                             NoneValueImporter<ExceptionInfo,
                                                                                                                               PyObjectImporters.Tuple<PyObject, PyObject, PyObject,
                                                                                                                                                       PyObjectImporters.Runtime<PyObject>,
                                                                                                                                                       PyObjectImporters.Runtime<PyObject>,
                                                                                                                                                       PyObjectImporters.Runtime<PyObject>>>>,
                                                                                     PyObjectImporters.Runtime<PyObject>>>();

        return Task.Run(() =>
        {
            while (generator.MoveNext()) // TODO Restart log records reading loop on failure
            {
                var (dropCount, level, message, exceptionInfo) = generator.Current;

                if (dropCount > 0)
                {
                    // One could log a warning here:
                    //
                    //   logger.LogWarning("Dropped {DropCount} log messages due to buffer overflow.", dropCount);
                    //
                    // but the reason the messages are getting dropped in the first place is because
                    // this loop isn't keeping up due to the logger being slow or blocking. Adding
                    // to the misery wouldn't be helpful so issue a debug message instead.

                    Debug.WriteLine($"Dropped {dropCount} log messages due to log buffer overflow.");
                }

                try
                {
                    LogRecord(logger, level, message, exceptionInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error logging Python log record: {ex}");
                }
            }
        });

        static void LogRecord(ILogger logger, long level, string message, ExceptionInfo? exceptionInfo)
        {
            Exception? exception = null;
            if (exceptionInfo is { } info)
            {
                using var pyExceptionType = info.ExceptionType.NoneToNull();
                using var pyException = info.Exception.NoneToNull();
                using var traceback = info.Traceback.NoneToNull();
                using var name = pyExceptionType?.GetAttr("__name__");
                exception = new PythonInvocationException(name?.ToString() ?? "Exception", pyException, traceback, message);
            }

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
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;

        disposed = true;

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
            var completed = false;

            try
            {
                await listenerTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                completed = true;
            }
            catch (TimeoutException)
            {
                Debug.WriteLine("Timeout waiting for logging task to complete.");
            }

            if (completed)
            {
                try
                {
                    await listenerTask.ConfigureAwait(false);
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
}

file sealed class NoneValueImporter<T, TImporter> : IPyObjectImporter<T?>
    where T : struct
    where TImporter : IPyObjectImporter<T>
{
    private NoneValueImporter() { }

    static T? IPyObjectImporter<T?>.BareImport(PyObject obj) =>
        !obj.IsNone() ? TImporter.BareImport(obj) : null;
}
