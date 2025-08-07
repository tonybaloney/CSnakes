# Advanced Usage

## Handling very large integers

Python's `int` type is closer in structure to C#.NET's `System.Numerics.BigInteger` than to `System.Int64`. This means that when you are working with very large integers in Python, you may need to use the `BigInteger` type in C# to handle the results.

You can use this using the TypeConverter class to convert between `BigInteger` and `PyObject` instances. Here's an example of how you can call a Python function that returns a very large integer:

```csharp
using CSnakes.Runtime.Python;
using System.Numerics;

const string number = "12345678987654345678764345678987654345678765";
// Something that is too big for a long (I8)
BigInteger bignumber = BigInteger.Parse(number);

using (GIL.Acquire())
{
    using PyObject? pyObj = PyObject.From(bignumber);

    // Do stuff with the integer object
    // e.g. call a function with this as an argument

    // Convert a Python int back into a BigInteger like this..
    BigInteger integer = pyObj.As<BigInteger>();
}
```

## Free-Threading Mode

Python 3.13 introduced a new feature called "free-threading mode" which allows the Python interpreter to run in a multi-threaded environment without the Global Interpreter Lock (GIL). This is a significant change to the Python runtime and can have a big impact on the performance of Python code running in a multi-threaded environment.

CSnakes supports free-threading mode, but it is disabled by default.

To use free-threading you can use the `RedistributableLocator` from version Python 3.13 and request `freeThreaded` builds:

```csharp
var builder = Host.CreateApplicationBuilder();
var pb = builder.Services.WithPython()
  .WithHome(Environment.CurrentDirectory) // Path to your Python modules.
  .FromRedistributable("3.13", freeThreaded: true);
var app = builder.Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

Whilst free-threading mode is **supported** at a high-level from CSnakes, it is still an experimental feature in Python 3.13 and may not be suitable for all use-cases. Also, most Python libraries, especially those written in C, are not yet compatible with free-threading mode, so you may need to test your code carefully.

## Calling Python without the Source Generator

The Source Generator library is a useful tool for creating the boilerplate code to invoke a Python function from a `PythonEnvironment` instance and convert the types based on the type annotations in the Python function.

It is still possible to call Python code without the Source Generator, but you will need to write the boilerplate code yourself. Here's an example of how you can call a Python function without the Source Generator to call a Python function in a module called `test_basic`:

```python
def test_int_float(a: int, b: float) -> float:
    return a + b
```

The C# code to call this function needs to:

1. Convert the .NET types to `PyObject` instances and back.
1. Use the `GIL.Acquire()` method to acquire the Global Interpreter Lock for all conversions and calls to Python.
1. Use the `Import.ImportModule` method to import the module and store a reference once so that it can be used multiple times.
1. Dispose the module when it is no longer needed.

```csharp
using CSnakes.Runtime;
using CSnakes.Runtime.Python;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

public sealed class ExampleDirectIntegration : IDisposable
{
    private readonly PyObject module;

    private readonly ILogger<IPythonEnvironment> logger;

    internal TestBasicInternal(IPythonEnvironment env)
    {
        this.logger = env.Logger;
        using (GIL.Acquire())
        {
            logger.LogInformation("Importing module {ModuleName}", "test_basic");
            module = Import.ImportModule("test_basic");
        }
    }

    public void Dispose()
    {
        logger.LogInformation("Disposing module");
        module.Dispose();
    }

    public double TestIntFloat(long a, double b)
    {
        using (GIL.Acquire())
        {
            logger.LogInformation("Invoking Python function: {FunctionName}", "test_int_float");
            using var __underlyingPythonFunc = this.module.GetAttr("test_int_float");
            using PyObject a_pyObject = PyObject.From(a);
            using PyObject b_pyObject = PyObject.From(b);
            using var __result_pyObject = __underlyingPythonFunc.Call(a_pyObject, b_pyObject);
            return __result_pyObject.As<double>();
        }
    }
}
```

### Disabling the Source Generator

The Source Generator will only activate on files that are marked as `AdditionalFiles` in the project file. If you want to disable the Source Generator, you can remove the `AdditionalFiles` entry from the project file.

Alternatively , you can disable the Source Generator by setting the `DisableCSnakesRuntimeSourceGenerator` property in the project file:

```xml
<DisableCSnakesRuntimeSourceGenerator>true</DisableCSnakesRuntimeSourceGenerator>
```

## Hot Reload

CSnakes supports [hot reload](https://learn.microsoft.com/visualstudio/debugger/hot-reload?view=vs-2022) of Python code in Visual Studio and supported IDEs. 
This means that you can make changes to your Python code within the function body and see the changes reflected in your C# code without restarting the application.

This feature is enabled in the generated classes in CSnakes. When you make changes to the Python code, the modules are reloaded in the .NET runtime and subsequent calls to the Python code will use the new code.

To enable Hot Reload, see the [VS 2022](https://learn.microsoft.com/visualstudio/debugger/hot-reload?view=vs-2022) documentation.

### Limitations

Beyond the C# [limitations](https://learn.microsoft.com/visualstudio/debugger/supported-code-changes-csharp?view=vs-2022), Hot Reload would not support changes to the Python code which require additional changes to the C# such as :

- Removing functions
- Changing function signatures
- Changing the return type of a function
- Changing the type of a function argument
- Changing the name of a function
- Changing the name of a module

The Hot Reload feature is useful for iterating on the __body__ of a Python function, without having to restart the debugger or application.

## Disabling Signal Handlers in Python

By default, Python will install signal handlers for certain signals, such as `SIGINT` (Ctrl+C) and `SIGTERM`. This can interfere with the normal operation of your application, especially if you are using a framework that has its own signal handlers.
This means that signal handlers on C# code will not be called when the signal is received, and the Python code will handle the signal instead.

You can disable this behavior by using the `.DisableSignalHandlers()` method on the `IPythonEnvironment` instance:

```csharp
var builder = Host.CreateApplicationBuilder();
var pb = builder.Services.WithPython()
  .WithHome(Environment.CurrentDirectory)
  .FromRedistributable()
  .DisableSignalHandlers(); // Disable Python signal handlers
var app = builder.Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

## Native AOT Support

CSnakes supports Native AOT (Ahead-of-Time) compilation, which allows you to compile your C# application to native code for faster startup times and reduced memory footprint. For comprehensive information about Native AOT in .NET, see the [Microsoft Native AOT documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

### Requirements for Native AOT

Native AOT support in CSnakes **only works with source generated bindings**. This means:

- You must use Python files marked as `AdditionalFiles` in your project
- The source generator must be enabled (which is the default)
- You cannot use the manual Python binding approach described in [Calling Python without the Source Generator](#calling-python-without-the-source-generator)

#### Why Source Generator is Required

The limitation exists because casting Python objects to .NET containers like `Tuple`, `Dictionary`, `List`, or `Coroutine` requires reflection when done dynamically, which is not supported in Native AOT compilation. The source generator solves this by generating compiled bindings and reflection code at build time without using `System.Reflection`, making the generated code AOT-ready.

### Configuring Native AOT

To enable Native AOT in your CSnakes project, add the following property to your `.csproj` file:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

The `InvariantGlobalization` setting is typically required for AOT compatibility, as it reduces the application's dependencies on culture-specific data. For more details on configuration options, see [Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

### Example AOT Project

Here's a complete example of a CSnakes project configured for Native AOT:

**AOTConsoleApp.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <AdditionalFiles Include="*.py">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="1.*-*" />
  </ItemGroup>
</Project>
```

**aot_demo.py:**
```python
def cool_things() -> list[str]:
    return [
        "Python",
        "C#",
    ]
```

**Program.cs:**
```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var home = Path.Join(Environment.CurrentDirectory);
builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable("3.12");

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

RunQuickDemo(env);

static void RunQuickDemo(IPythonEnvironment env)
{
    var quickDemo = env.AotDemo();
    foreach (var thing in quickDemo.CoolThings())
    {
        Console.WriteLine(thing + " is cool!");
    }
    Console.WriteLine();
}
```

### Publishing for Native AOT

To publish your application with Native AOT, use the following command:

```bash
dotnet publish -c Release -r <runtime-identifier>
```

For example, to publish for Windows x64:
```bash
dotnet publish -c Release -r win-x64
```

For more information about publishing Native AOT applications and runtime identifiers, see the [Publishing Native AOT apps guide](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

#### Python Environment Packaging

**Important**: While `dotnet publish` builds a self-contained application, that application does not contain Python, the Python virtual environment, or its dependencies. The published application will still require a Python runtime and any required packages to be available on the target system.

For packaging Python environments alongside your application, see the documentation on the [`CSnakes.Stage` tool](https://tonybaloney.github.io/CSnakes/docker/) which provides guidance on bundling Python environments with your .NET applications.

### Limitations and Considerations

1. **Source Generator Required**: Native AOT only works with the source generator. Manual Python binding using the runtime API directly is not supported in AOT scenarios due to reflection limitations when dynamically casting Python objects to .NET containers.

2. **Reflection Limitations**: Native AOT has limited support for reflection, which is why the source generator approach is mandatory. For more details, see [Native AOT compatibility requirements](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/compatibility).

3. **Python Runtime**: The Python runtime itself is not compiled to native code - only your C# application is. The Python interpreter still runs in its normal mode.

4. **Debugging**: Debugging AOT-compiled applications can be more challenging than debugging JIT-compiled applications. See [Native AOT debugging](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/debugging) for more information.

5. **Build Time**: Native AOT compilation takes longer than regular compilation.

6. **File Size**: While memory usage may be reduced, the resulting executable may be larger due to including the entire runtime.

### Sample Project

A complete working example of Native AOT with CSnakes is available in the [samples/simple/AOTConsoleApp](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/AOTConsoleApp) directory of the repository. This sample demonstrates the proper configuration and usage patterns for AOT deployment.
