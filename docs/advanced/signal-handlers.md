# Signal Handler Configuration

By default, Python will install signal handlers for certain signals, such as `SIGINT` (Ctrl+C) and `SIGTERM`. This can interfere with the normal operation of your application, especially if you are using a framework that has its own signal handlers.

When Python handles these signals instead of your .NET application, it can prevent proper shutdown procedures and interrupt normal signal processing in your C# code.

## Understanding the Issue

### Default Python Behavior

Python automatically installs signal handlers for:

- **SIGINT** (Ctrl+C) - Keyboard interrupt
- **SIGTERM** - Termination request
- **SIGPIPE** - Broken pipe
- **Other signals** depending on the platform

This means that signal handlers on C# code will not be called when the signal is received, and the Python code will handle the signal instead.

### Impact on .NET Applications

```csharp
// This C# signal handler might not work as expected
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("C# handling Ctrl+C");
    // This might not be called if Python handles SIGINT first
    e.Cancel = true;
};
```

## Disabling Python Signal Handlers

You can disable this behavior by using the `.DisableSignalHandlers()` method on the Python environment configuration:

```csharp
var builder = Host.CreateApplicationBuilder();
var pb = builder.Services.WithPython()
  .WithHome(Environment.CurrentDirectory)
  .FromRedistributable()
  .DisableSignalHandlers(); // Disable Python signal handlers
var app = builder.Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

**Note:** You can customize the Python cache location by setting the `CSNAKES_REDIST_CACHE` environment variable to override the default application data folder.

## Next Steps

- [Learn about Native AOT support](native-aot.md)
- [Review troubleshooting guide](troubleshooting.md)
