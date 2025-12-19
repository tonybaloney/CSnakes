# Free-Threading Mode

Python 3.13 introduced a new feature called "free-threading mode" which allows the Python interpreter to run in a multi-threaded environment without the Global Interpreter Lock (GIL). This is a significant change to the Python runtime and can have a big impact on the performance of Python code running in a multi-threaded environment.

## Overview

The Global Interpreter Lock (GIL) has historically been a limitation in Python's ability to utilize multiple CPU cores effectively in multi-threaded applications. Free-threading mode removes this limitation, allowing Python code to run truly in parallel across multiple threads.

## Enabling Free-Threading in CSnakes

CSnakes supports free-threading mode, but it is disabled by default. To use free-threading you can use the `RedistributableLocator` from version Python 3.13 and request `freeThreaded` builds:

```csharp
var builder = Host.CreateApplicationBuilder();
var pb = builder.Services.WithPython()
  .WithHome(Environment.CurrentDirectory) // Path to your Python modules.
  .FromRedistributable("3.13", freeThreaded: true);
var app = builder.Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

## Requirements

- **Python 3.13 or later**: Free-threading is only available in Python 3.13+
- **Compatible libraries**: Most Python libraries need updates to work with free-threading
- **Testing required**: Thorough testing is essential as this is still experimental

## Important Considerations

Whilst free-threading mode is **supported** at a high-level from CSnakes, it is still an experimental feature in Python 3.13 and may not be suitable for all use-cases. Also, most Python libraries, especially those written in C, are not yet compatible with free-threading mode, so you may need to test your code carefully.

## Next Steps

- [Explore manual Python integration](manual-integration.md)
- [Learn about hot reload](hot-reload.md)
- [Review performance optimization](performance.md)
