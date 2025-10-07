# Logging

## Enabling C# logs

CSnakes uses the Host Builder pattern which includes a logger factory. CSnakes will resolve the logger from the factory, for example if you want debug logs to be written to the console:

```csharp
// Configure logging in Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

CSnakes raises debug logs for tasks like imports and package installs. Reduce the log level to Warning if you only want important log entries on the console.

## Bringing Python logs into C#

Because Python and C#.NET have different logging systems, CSnakes offers a way to capture Python's logs and emit them into the .NET logger APIs.

This works by installing a custom log handler in Python at runtime and emitting log records back into .NET.

You can enable this feature globally using the `.CapturePythonLogs()` extension method:

```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable()
    // Existing options
    .CapturePythonLogs(); // Setup global log capture

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();
```

Or, if you only want it for a specific task or function call, it is available on the `IPythonEnvironment` interface:

```csharp
public void YourCode(IPythonEnvironment env, ILogger logger)
{
    using (env.WithPythonLogging(logger)) // Enable logging for this scope
    {
        var mod = env.MyModule();
        mod.MyPythonFunction();
    }
}
```

If the Python function in `my_module.py` emits log entries:

```python
import logging

def my_python_function():
    logging.info("This is a message from Python")
```

The log entries will be emitted to the `ILogger` interface provided.

If you want to be even more granular, the `WithPythonLogging()` method on `IPythonEnvironment` accepts the name of the logger. By default, the root logger in Python is used (when `loggerName` is `null`), but if you wanted to only capture a named logger:

```csharp
public void YourCode(IPythonEnvironment env, ILogger logger)
{
    using (env.WithPythonLogging(logger, "csnakes_logger")) // Enable logging for specific logger
    {
        var mod = env.MyModule();
        mod.MyPythonFunction();
    }
}
```

Then in Python, only the `"csnakes_logger"` will emit entries back to .NET:

```python
import logging

logger = logging.getLogger("csnakes_logger")

logger.info("Only this logger will send back")
```

## Log Level Mapping

Python and .NET have different logging levels. CSnakes maps them as follows:

| Python Level | Python Value | .NET `LogLevel` |
| :----------: | :----------: | :-------------: |
|  `CRITICAL`  |     50+      |   `Critical`    |
|   `ERROR`    |    40-49     |     `Error`     |
|  `WARNING`   |    30-39     |    `Warning`    |
|    `INFO`    |    20-29     |  `Information`  |
|   `DEBUG`    |    10-19     |     `Debug`     |
|   `NOTSET`   |     0-9      |    (ignored)    |

## Logging raised exceptions

Sometimes you want to capture exceptions before they're raised, or capture and log them before continuing with program operation.

Python's loggers have a `.exception()` method which captures the current exception and stack trace:

```python
try:
    1 / 0  # Example of an exception
except Exception:
    logging.exception("An error message occurred")
```

This exception object will be converted to a `PythonInvocationException` and passed as the exception parameter to the .NET logger. See [Exception Handling](errors.md) for examples on using those exception objects.

## Performance Considerations

- The Python logging bridge runs on a background task to avoid blocking Python execution
- Log records are queued and processed asynchronously
- When using scoped logging with `WithPythonLogging()`, the handler is automatically removed when the scope is disposed
- Global logging (via `CapturePythonLogs()`) remains active for the lifetime of the Python environment

## Troubleshooting

If Python logs are not appearing:

1. Ensure the Python logger level is set appropriately:
   ```python
   import logging
   logging.getLogger().setLevel(logging.DEBUG)
   ```

2. Check that the .NET logger is configured to accept the log level:
   ```csharp
   builder.Logging.SetMinimumLevel(LogLevel.Debug);
   ```

3. When using named loggers, ensure you're using the exact same name in both Python and C#
