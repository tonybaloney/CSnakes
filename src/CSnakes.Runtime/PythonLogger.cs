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

    private static ICsnakesLogging GetModule(IPythonEnvironment env)
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
                ((PythonEnvironment)env).AddDisposalListener(result, static (_, subscription, module) =>
                {
                    module.Dispose();
                    subscription.Dispose();
                });
            }

            return result;
        }
    }
}

file sealed class Bridge(PyObject handler, ICsnakesLogging module, Task listenerTask) : IAsyncDisposable
{
    private bool disposed;

    internal static Bridge Create(ICsnakesLogging module, ILogger logger, string? loggerName = null)
    {
        PyObject? handler = null;
        Task listenerTask;

        try
        {
            using (GIL.Acquire())
            {
                handler = module.NewHandler();
                module.InstallHandler(handler, loggerName);
                listenerTask = StartRecordListener(module, handler, logger);
            }
        }
        catch
        {
            handler?.Dispose();
            throw;
        }

        return new(handler, module, listenerTask);
    }

    private static Task StartRecordListener(ICsnakesLogging module, PyObject handler, ILogger logger)
    {
        var generator = module.GetRecords(handler);

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
            module.UninstallHandler(handler);
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
    }
}
