# Advanced Usage

## Free-Threading Mode

Python 3.13 introduced a new feature called "free-threading mode" which allows the Python interpreter to run in a multi-threaded environment without the Global Interpreter Lock (GIL). This is a significant change to the Python runtime and can have a big impact on the performance of Python code running in a multi-threaded environment.

CSnakes supports free-threading mode, but it is disabled by default. 

To use free-threading you currently need to compile CPython from source, so the free-threading flag is only available in the `SourceLocator`.

Here's how you would compile CPython 3.13 on Windows with free-threading and use it from CSnakes:

```cmd
git clone git@github.com:python/cpython.git
cd cpython
git checkout 3.13
PCBuild\build.bat -t build -c Release --disable-gil
```

Then you can use the `SourceLocator` via the `.FromSource()` extension method to find the compiled Python runtime:

```csharp
app = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        var pb = services.WithPython();
        pb.WithHome(Environment.CurrentDirectory); // Path to your Python modules. 
        pb.FromSource(@"C:\path\to\cpython\", false, true);
    })
    .Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

Whilst free-threading mode is **supported** at a high-level from CSnakes, it is still an experimental feature in Python 3.13 and may not be suitable for all use-cases. Also, most Python libraries, especially those written in C, are not yet compatible with free-threading mode, so you may need to test your code carefully.
