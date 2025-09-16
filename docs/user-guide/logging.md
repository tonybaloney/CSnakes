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

You can enable this feature globally, use the `.CapturePythonLogs()` extension method:

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

public void YourCode(IPythonEnvironment env, ILogging logger) {
    using (env.WithPythonLogging(logger)) // Enable logging
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

If you want to be even more granular, the `WithPythonLogging()` method on `IPythonEnvironment` accepts the name of the logger. By default, the root logger in Python is used, but if you wanted to only use a named logger:

```csharp
public void YourCode(IPythonEnvironment env, ILogging logger) {
    using (env.WithPythonLogging(logger, "csnakes_logger")) // Enable logging
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


## Logging raised exceptions

Sometimes you want to capture exceptions before they're raised, or capture and log them before continuing with program operation.

Python's loggers have a `.exception()` method which captures the current exception and stack trace:

```python
try:
    1 / 0  # Example of an exception
except Exception:
    logging.exception("An error message occurred")

```

This exception object will be converted to a `PythonInvocationException`, see [Exception Handling](errors.md) for examples on using those exception objects.
