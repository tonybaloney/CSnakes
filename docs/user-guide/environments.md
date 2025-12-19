# Environment and Package Management

CSnakes comes with support for executing Python within a virtual environment and the specification of dependencies.

There are two main package management solutions for Python, `pip` and `conda`. `pip` is the default package manager for Python and is included with the Python installation. `conda` is a package manager that is included with the Anaconda distribution of Python. Both package managers can be used to install packages and manage dependencies.

There are various ways to create "virtual" environments in Python, where the dependencies are isolated from the system Python installation. The most common way is to use the `venv` module that is included with Python. The `venv` module is used to create virtual environments and manage dependencies. 

Virtual Environment creation and package management are separate concerns in Python, but some tools (like conda) combine them into a single workflow. CSnakes separates these concerns to give you more flexibility in managing your Python environments.

## Virtual Environments with `venv`

Use the `.WithVirtualEnvironment(path)` method to specify the path to the virtual environment.

You can also optionally use the `.WithPipInstaller()` method to install packages listed in a `requirements.txt` file in the virtual environment. If you don't use this method, you need to install the packages manually before running the application.

```csharp
...
services
    .WithPython()
    .WithVirtualEnvironment(Path.Join(home, ".venv"))
    // Python locators
    .WithPipInstaller(); // Optional - installs packages listed in requirements.txt on startup
```

### Disabling automatic environment creation

## Virtual Environments with `conda`

To use the `conda` package manager, you need to specify the path to the `conda` executable and the name of the environment you want to use:

1. Add the `FromConda()` extension method the host builder. 
1. Use the `.WithCondaEnvironment(name)` method to specify the name of the environment you want to use.

```csharp
...
services
    .WithPython()
    .FromConda(condaBinPath)
    .WithCondaEnvironment("name_of_environment");
```

The Conda Environment manager doesn't currently support automatic creation of environments or installing packages from an `environment.yml` file, so you need to create the environment and install the packages manually before running the application, by using `conda env create -n name_of_environment -f environment.yml`

## Installing dependencies with `pip`

If you want to install dependencies using `pip`, you can use the `.WithPipInstaller()` method. This method will install the packages listed in a `requirements.txt` file in the virtual environment.

```csharp
...
services
    .WithPython()
    .WithVirtualEnvironment(Path.Join(home, ".venv"))
    .WithPipInstaller(); // Optional - installs packages listed in requirements.txt on startup
```

`.WithPipInstaller()` takes an optional argument that specifies the path to the `requirements.txt` file. If you don't specify a path, it will look for a `requirements.txt` file in the virtual environment directory.

## Installing dependencies with `uv`

`uv` is an alternative to pip that can also install requirements from a file like `requirements.txt` or `pyproject.toml`. UV has a major benefit in a 10-100x speedup over pip, so your CSnakes applications will be faster to start.

To use uv to install packages:

```csharp
...
services
    .WithPython()
    .WithVirtualEnvironment(Path.Join(home, ".venv"))
    .WithUvInstaller("requirements.txt"); // Optional - give the name of the requirements file, or pyproject.toml
```

Some other important notes about this implementation.

- Only uses uv to install packages and does not use uv to create projects or virtual environments.
- Must be used with `WithVirtualEnvironment()`, as pip requires a virtual environment to install packages into.
- Will use the `UV_CACHE_DIR` environment variable to cache the packages in a directory if set.
- Will disable the cache if the `UV_NO_CACHE` environment variable is set.

## Installing packages at runtime

You can resolve the `IPythonPackageInstaller` service to install packages in the virtual environment. This is useful if you want to install a package at runtime without having to modify the `requirements.txt` file.

```csharp
services // As existing (see above examples)
    .WithPython()
    .WithVirtualEnvironment(Path.Join(home, ".venv"))
    .WithPipInstaller(); // Optional - installs packages listed in requirements.txt on startup

// Environment/host builder setup

var installer = serviceProvider.GetRequiredService<IPythonPackageInstaller>();
await installer.InstallPackage("attrs==25.3.0");
```

This requires both an environment manager (UV or venv) and a package installer to be set up, as it uses the same mechanisms to install packages.

Optionally, you can install multiple packages at once by passing a list of package names:

```csharp
await installer.InstallPackages(new[] { "attrs==25.3.0", "requests==2.31.0" });
```

Or, if the package names are in a file, you can use:

```csharp
await installer.InstallPackagesFromRequirements("requirements.txt");
```
