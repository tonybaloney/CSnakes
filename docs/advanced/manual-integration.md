# Manual Python Integration

The Source Generator library is a useful tool for creating the boilerplate code to invoke a Python function from a `PythonEnvironment` instance and convert the types based on the type annotations in the Python function.

However, it is still possible to call Python code without the Source Generator, giving you full control over the integration. This approach requires writing the boilerplate code yourself but provides maximum flexibility.

## When to Use Manual Integration

Consider manual integration when:

- You need fine-grained control over Python object handling
- Working with dynamic Python code that can't be statically analyzed
- Implementing custom type conversions not supported by the source generator
- Debugging complex interop scenarios
- Building custom abstractions over the CSnakes runtime

Importantly: 

- Dynamic conversion will not be supported in Native AOT
- In most scenarios, manual integration will be slower

## Basic Manual Integration Example

Here's an example of how you can call a Python function without the Source Generator. Consider this Python function in a module called `test_basic`:

```python
def test_int_float(a: int, b: float) -> float:
    return a + b
```

The C# code to call this function manually needs to:

1. Convert the .NET types to `PyObject` instances and back
2. Use the `GIL.Acquire()` method to acquire the Global Interpreter Lock for all conversions and calls to Python
3. Use the `Import.ImportModule` method to import the module and store a reference once so that it can be used multiple times
4. Dispose the module when it is no longer needed

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

    internal ExampleDirectIntegration(IPythonEnvironment env)
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

## Disabling the Source Generator

If you want to disable the Source Generator completely, you have two options:

### Option 1: Remove AdditionalFiles

The Source Generator will only activate on files that are marked as `AdditionalFiles` in the project file. Remove the `AdditionalFiles` entry:

```xml
<!-- Remove or comment out this section -->
<!--
<ItemGroup>
  <AdditionalFiles Include="*.py" SourceItemType="Python"/>
</ItemGroup>
-->
```

## Best Practices for Manual Integration

### 1. Proper Resource Disposal

```csharp
// ✅ Good - Proper disposal
using (GIL.Acquire())
{
    using var arg1 = PyObject.From(value1);
    using var arg2 = PyObject.From(value2);
    using var result = function.Call(arg1, arg2);
    return result.As<string>();
}

// ❌ Bad - Memory leaks
var arg1 = PyObject.From(value1);
var arg2 = PyObject.From(value2);
var result = function.Call(arg1, arg2);
// Objects not disposed - will cause memory leaks!
```

### 2. Error Handling

```csharp
try
{
    using (GIL.Acquire())
    {
        // Python calls here
    }
}
catch (PythonInvocationException ex)
{
    // Handle Python-specific exceptions
    logger.LogError(ex, "Python error: {PythonType}", ex.PythonExceptionType);
}
catch (Exception ex)
{
    // Handle other exceptions
    logger.LogError(ex, "Unexpected error in Python integration");
}
```

### 4. Caching

```csharp
// Cache modules, attributes and other classes instead of calling `GetAttr` each time.
private readonly PyObject module;

public MyClass(IPythonEnvironment env)
{
    using (GIL.Acquire())
    {
        module = Import.ImportModule("my_module");
    }
}
```

## Next Steps

- [Learn about hot reload](hot-reload.md)
- [Explore Native AOT support](native-aot.md)
- [Review performance optimization](performance.md)
