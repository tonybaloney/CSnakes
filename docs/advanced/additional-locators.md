# Additional Python Locators

CSnakes uses a `PythonLocator` to find the Python runtime on the host machine. The `PythonLocator` is a service that is registered with the dependency injection container and is used to find the Python runtime on the host machine.

You can chain locators together to match use the first one that finds a Python runtime. This is a useful pattern for code that is designed to run on Windows, Linux, and MacOS.

The simplest and most user-friendly locator is the Redistributable Locator. This will fetch and run Python for you.

## Redistributable Locator

The `.FromRedistributable()` method automates the installation of a compatible version of Python. It will source Python and cache it locally. This download is about 50-80MB, so the first time you run your application, it will download the redistributable and cache it locally. The next time you run your application, it will use the cached redistributable. This could take a minute or two depending on your bandwidth.

By default, Python 3.12 will be used and will be installed in the [application data](https://learn.microsoft.com/en-us/dotnet/api/system.environment.specialfolder?view=net-9.0) which depends on your operating system. You can change this location by setting the `CSNAKES_REDIST_CACHE` environment variable. Make sure the user running the application has permission to write to this folder.

To specify a different major version of Python:

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromRedistributable("3.13");
```

You can also request debug builds on macOS and Linux to aid with debugging of crashes in native extensions:

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromRedistributable("3.13", debug: true);
```

Or, if you want to experiment with free-threaded builds of Python, the free-threaded build for Python 3.13+:

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromRedistributable("3.13", debug: true, freeThreaded: true);
```

## Environment Variable Locator

The `.FromEnvironmentVariable()` method allows you to specify an environment variable that contains the path to the Python runtime. This is useful for scenarios where the Python runtime is installed in a non-standard location or where the path to the Python runtime is not known at compile time.

This locator is also very useful for GitHub Actions `setup-python` actions, where the Python runtime is installed in a temporary location specified by the environment variable "`Python3_ROOT_DIR`":

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromEnvironmentVariable("Python3_ROOT_DIR", "3.12");
```

## Folder Locator

The `.FromFolder()` method allows you to specify a folder that contains the Python runtime. This is useful for scenarios where the Python runtime is installed in a known location on the host machine.

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromFolder(@"C:\path\to\python\3.12", "3.12");
```

## Source Locator

The Source Locator is used to find a compiled Python runtime from source. This is useful for scenarios where you have compiled Python from source and want to use the compiled runtime with CSnakes.

It optionally takes a `bool` parameter to specify that the binary is debug mode and to enable free-threading mode in Python 3.13:

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromSource(@"C:\path\to\cpython\", "3.13", debug: true,  freeThreaded: true);
```

## MacOS Installer Locator

The MacOS Installer Locator is used to find the Python runtime on MacOS. This is useful for scenarios where you have installed Python from the official Python installer on MacOS from [python.org](https://www.python.org/downloads/).

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromMacOSInstaller("3.12");
```

## Windows Installer Locator

The Windows Installer Locator is used to find the Python runtime on Windows. This is useful for scenarios where you have installed Python from the official Python installer on Windows from [python.org](https://www.python.org/downloads/).

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromWindowsInstaller("3.12");
```

## Windows Store Locator

The Windows Store Locator is used to find the Python runtime on Windows from the Windows Store. This is useful for scenarios where you have installed Python from the Windows Store on Windows.

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromWindowsStore("3.12")
```

## Nuget Locator

The Nuget Locator is used to find the Python runtime from a Nuget package. This is useful for scenarios where you have installed Python from one of the Python Nuget packages found at [nuget.org](https://www.nuget.org/packages/python/).

These packages only bundle the Python runtime for Windows. You also need to specify the minor version of Python:

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromNuGet("3.12.4");
```

## Conda Locator

The Conda Locator is used to find the Python runtime from a Conda environment. This is useful for scenarios where you have installed Python from the Anaconda or miniconda distribution of Python. Upon environment creation, CSnakes will run `conda info --json` to get the path to the Python runtime.

This Locator should be called with the path to the Conda executable:

```csharp
...
var pythonBuilder = services.WithPython()
                            .FromConda(@"C:\path\to\conda");
```

The Conda Locator should be combined with the `WithCondaEnvironment` method to specify the name of the Conda environment you want to use. See [Environment and Package Management](environments.md) for more information on managing Python environments and dependencies.
