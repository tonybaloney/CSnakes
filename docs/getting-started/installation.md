# Installation (manual setup)

To get started with CSnakes manually, you need to:

* [Install Python](#installing-python)
* [Install the CSnakes.Runtime NuGet package](#installing-the-nuget-package)
* [Configure your C# project](#configuring-a-c-project)

## Installing Python

CSnakes supports Python 3.9-3.13 and works on Windows, macOS, and Linux.

### Option 1: Let CSnakes Download Python (Recommended)

The simplest option is to use the `FromRedistributable` method, which will automatically download Python 3.12 and store it locally. This is compatible with Windows, macOS, and Linux.

```csharp
builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable(); // Downloads Python 3.12 automatically
```

By default, the downloaded Python files are cached in your user's application data folder (`%APPDATA%\CSnakes` on Windows, `~/.local/share/CSnakes` on Linux, etc.). You can override this location by setting the `CSNAKES_REDIST_CACHE` environment variable:

```bash
# Windows
set CSNAKES_REDIST_CACHE=C:\MyCustomCache

# Linux/macOS  
export CSNAKES_REDIST_CACHE=/path/to/custom/cache
```

### Option 2: Use System Python

If you have Python installed on your system, you can use it directly:

```csharp
builder.Services
    .WithPython()
    .WithHome(home)
    .FromEnvironmentVariable("PYTHONHOME", "3.12"); // Uses system Python
```

### Option 3: Use Conda

If you're using Conda environments:

```csharp
builder.Services
    .WithPython()
    .WithHome(home)
    .FromConda(condaBinPath); // Uses `conda` at specified path
```

## Installing the NuGet Package

CSnakes is bundled into a single NuGet package: [`CSnakes.Runtime`](https://www.nuget.org/packages/CSnakes.Runtime/).

### Using Package Manager Console

```powershell
Install-Package CSnakes.Runtime
```

### Using .NET CLI

```bash
dotnet add package CSnakes.Runtime
```

### Using PackageReference

Add this to your `.csproj` file:

```xml
<PackageReference Include="CSnakes.Runtime" Version="2.*-*" />
```

This package includes both the source generator and runtime libraries.

## Configuring a C# Project

To setup a C# project for CSnakes, you need to:

1. Create a new C# project or open an existing one
2. Add your Python files to the project
3. Mark the Python files as "Additional Files" in the project file
4. Install the `CSnakes.Runtime` NuGet package
5. Create a `PythonEnvironment` in C# and create an instance of the Python module

### Requirements

- .NET 8 or 9
- Python 3.9-3.13
- Windows, macOS, or Linux

## Next Steps

- [Create your first example](first-example.md)
- [Learn about Python environment management](../user-guide/environments.md)
- [Understand the type system](../user-guide/type-system.md)
