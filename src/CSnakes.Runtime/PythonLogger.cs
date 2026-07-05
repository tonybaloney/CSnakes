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
    static readonly Lock ModuleLock = new();
    static ICsnakesLogging? module;

    public static IAsyncDisposable WithPythonLogging(this IPythonEnvironment env, ILogger logger, string? loggerName = null) =>
        Bridge.Create(GetModule(env), logger, loggerName);

    internal static IAsyncDisposable EnableGlobalLogging(IPythonEnvironment env, ILogger logger) =>
        Bridge.Create(GetModule(env), logger);

    private static ICsnakesLogging GetModule(IPythonEnvironment env) =>
        GetModule((PythonEnvironment)env);

    private static ICsnakesLogging GetModule(PythonEnvironment env)
    {
        lock (ModuleLock)
        {
            ICsnakesLogging result;

            if (module is { } someModule)
            {
                result = someModule;
            }
            else
            {
                result = module = env.CsnakesLogging();

                void OnDisposing(object? sender, EventArgs args)
                {
                    env.Disposing -= OnDisposing;

                    lock (ModuleLock)
                    {
                        Debug.Assert(module == result);
                        module = null;
                    }

                    result.Dispose();
                }

                env.Disposing += OnDisposing;
            }

            return result;
        }
    }
}

file sealed class Bridge(PyObject closeCallable, Task listenerTask) : IAsyncDisposable
{
    private bool disposed;

    internal static Bridge Create(ICsnakesLogging module, ILogger logger, string? loggerName = null)
    {
        PyObject? closeCallable = null;
        Task listenerTask;

        try
        {
            using (GIL.Acquire())
            {
                var result = module.Monitor(loggerName);
                closeCallable = result.Close;
                listenerTask = StartRecordListener(result.Generator,
                                                   static r =>
                                                       (checked((int)r.DropCount),
                                                        // https://docs.python.org/3/library/logging.html#levels
                                                        (r.Level / 10) switch
                                                        {
                                                            1 => LogLevel.Debug,
                                                            2 => LogLevel.Information,
                                                            3 => LogLevel.Warning,
                                                            4 => LogLevel.Error,
                                                            >= 5 => LogLevel.Critical,
                                                            _ => LogLevel.None,
                                                        },
                                                        r.Message,
                                                        r.ExceptionInfo),
                                                   logger);
            }
        }
        catch
        {
            if (closeCallable is { } callable)
            {
                callable.Call().Dispose();
                callable.Dispose();
            }
            throw;
        }

        return new(closeCallable, listenerTask);
    }

    private static Task
        StartRecordListener<T>(
            IEnumerator<T> enumerator,
            Func<T, (int DropCount, LogLevel Level, string Message, ExceptionInfo? ExceptionInfo)> selector,
            ILogger logger)
    {
        return Task.Run(() =>
        {
            while (enumerator.MoveNext()) // TODO Restart log records reading loop on failure
            {
                var record = selector(enumerator.Current);

                if (record is { DropCount: > 0 and var dropCount })
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
                    LogRecord(logger, record.Level, record.Message, record.ExceptionInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error logging Python log record: {ex}");
                }
            }
        });

        static void LogRecord(ILogger logger, LogLevel level, string message, ExceptionInfo? exceptionInfo)
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

            if (level == LogLevel.None || !logger.IsEnabled(level))
                return;

            logger.Log(level, exception, message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
            return;

        disposed = true;

        var closed = false;

        try
        {
            closeCallable.Call();
            closed = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during uninstallation of handler: {ex}");
        }

        if (closed)
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

        closeCallable.Dispose();
    }
}
