# Getting Started

To get started with CSnakes, you need to:

* [Install Python](#installing-python)
* [Put your Python files into a C# Project](#configuring-a-c-project-for-csnakes)
* [Use type annotations for the functions you want to call from C#](#using-type-annotations-for-reflection)
* [Install the `CSnakes.Runtime` package into the project](#installing-the-nuget-packages-for-csnakes)
* [Mark them for source generation](#marking-files-for-generation)
* [Install the `CSnakes.Runtime` nuget package in the C# Project you want to execute the Python code from](#building-the-project)
* [Setup a Virtual Environment (Optional)](#using-virtual-environments)
* [Instantiate a Python environment in C# and run the Python function](#calling-csnakes-code-from-cnet)

## Configuring a C# Project for CSnakes

To setup a C# project for CSnakes, you need to:

1. Create a new C# project or open an existing one.
2. Add your Python files to the project.
3. Mark the Python files as "Additional Files" in the project file.
4. Install the `CSnakes.Runtime` NuGet package.
5. Create a `PythonEnvironment` in C# and create an instance of the Python module.
6. Call any Python code. 

### Installing the NuGet package

CSnakes is bundled into a single nuget package, [`CSnakes.Runtime`](https://www.nuget.org/packages/CSnakes.Runtime/). This package includes the source generator and runtime libraries.

Source Generation is recommended for the best experience, but you can also use the runtime library directly if you prefer, see [Calling Python without the Source Generator](advanced.md#calling-python-without-the-source-generator).

## Adding Python files

CSnakes uses the Python type annotations to generate C# code. You need to add Python files to your project and add type annotations to the functions you want to call from C#.

For example, if you were to create a Python file called `demo.py` to your project with the following content: 

```python
def hello_world(name: str) -> str:
    return f"Hello, {name}!"
```

See the [reference supported types](reference.md#supported-types) for a list of Python types and their C#.NET equivalents.

## Marking files for generation

For CSnakes to run the source generator over Python files, you either need to mark the Python files as an "Additional File" in the CSProj file XML:

```xml
    <ItemGroup>
        <AdditionalFiles Include="demo.py">
            <CopyToOutputDirectory>Always</CopyToOutputDirectory>
        </AdditionalFiles>
    </ItemGroup>
```

MSBuild supports globbing, so you can also mark all Python files in a directory as additional files:

```xml
    <ItemGroup>
        <AdditionalFiles Include="*.py">
            <CopyToOutputDirectory>Always</CopyToOutputDirectory>
        </AdditionalFiles>
    </ItemGroup>
```

Alternatively, you can set CSnakes to parse all `*.py` files by default, by setting the `DefaultPythonItems` property to
`true` in the project file:

```xml
    <PropertyGroup>
        <DefaultPythonItems>true</DefaultPythonItems>
    </PropertyGroup>
```

## Building the project

After you've added the Python files to the project, installed the NuGet packages, and marked the Python files for generation, you can build the project using `dotnet build`.

## Constructing a Python environment from C#

Python environments created by CSnakes are designed to be process-level singletons. This means that you can create a Python environment once and use it throughout the lifetime of the process.

CSnakes comes with a host builder for the `Microsoft.Extensions.Hosting` library to make it easier to create a Python environment in your C# code.

CSnakes also needs to know where to find Python using one or many [Python Locators](reference.md#python-locators).
The simplest option is the [`FromRedistributable`](reference.md#redistributable-locator) extension method, which will download Python 3.12 and store it locally. This is compatible with Windows, macOS, and Linux.

Here's an example of how you can create a Python environment in C#:

```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
var home = Path.Join(Environment.CurrentDirectory, "."); /* Path to your Python modules */
builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable(); // Download Python 3.12 and store it locally

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();
```

Check out the sample project in the [samples](https://github.com/tonybaloney/CSnakes/tree/main/samples) for code examples of a .NET console application and web application.

## Using Virtual Environments and Python Requirements

Since most Python projects __require__ external dependencies outside of the Python standard library, CSnakes supports execution within a Python virtual environment and Conda environments.

Use the `.WithVirtualEnvironment` method to specify the path to the virtual environment.

You can also optionally use the `.WithPipInstaller()` method to install packages listed in a `requirements.txt` file in the virtual environment. If you don't use this method, you need to install the packages manually before running the application.

```csharp
var home = Path.Join(Environment.CurrentDirectory, "."); /* Path to your Python modules */
builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable()

    /* Add these methods to configure a virtual environment and install packages from requirements.txt */
    .WithVirtualEnvironment(Path.Join(home, ".venv"))
    .WithPipInstaller(); // Optional - installs packages listed in requirements.txt on startup
```

See [Environment and Package Management](environments.md) for more information on managing Python environments and dependencies.

## Calling CSnakes code from C#.NET

Once you have a Python environment, you can call any Python function from C# using the `IPythonEnvironment` interface.

Here's an example of how you can call the `hello_world` function from the `demo.py` file:

```csharp
var env = app.Services.GetRequiredService<IPythonEnvironment>();

var module = env.Demo();

var result = module.HelloWorld("Alice");
Console.WriteLine(result); // Hello, Alice!
```

Check out the [reference](reference.md) for more information on supported types and features.
