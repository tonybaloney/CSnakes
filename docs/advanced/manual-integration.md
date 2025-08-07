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

## Advanced Manual Integration Patterns

### Dynamic Function Calling

When you don't know the function names at compile time:

```csharp
public class DynamicPythonCaller : IDisposable
{
    private readonly PyObject module;
    private readonly IPythonEnvironment environment;

    public DynamicPythonCaller(IPythonEnvironment env, string moduleName)
    {
        environment = env;
        using (GIL.Acquire())
        {
            module = Import.ImportModule(moduleName);
        }
    }

    public T CallFunction<T>(string functionName, params object[] args)
    {
        using (GIL.Acquire())
        {
            using var function = module.GetAttr(functionName);
            using var pyArgs = ConvertArguments(args);
            using var result = function.Call(pyArgs.ToArray());
            return result.As<T>();
        }
    }

    public async Task<T> CallFunctionAsync<T>(string functionName, params object[] args)
    {
        return await Task.Run(() => CallFunction<T>(functionName, args));
    }

    public bool HasFunction(string functionName)
    {
        using (GIL.Acquire())
        {
            return module.HasAttr(functionName);
        }
    }

    private List<PyObject> ConvertArguments(object[] args)
    {
        var pyArgs = new List<PyObject>();
        foreach (var arg in args)
        {
            pyArgs.Add(PyObject.From(arg));
        }
        return pyArgs;
    }

    public void Dispose()
    {
        module?.Dispose();
    }
}
```

### Custom Type Conversion

Implement custom conversions for complex types:

```csharp
public class CustomTypeConverter
{
    public static PyObject ToNumpyArray(double[,] matrix)
    {
        using (GIL.Acquire())
        {
            // Convert 2D array to Python list of lists
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);
            
            using var numpy = Import.ImportModule("numpy");
            using var arrayFunc = numpy.GetAttr("array");
            
            // Create list of lists
            var pythonMatrix = new List<List<double>>();
            for (int i = 0; i < rows; i++)
            {
                var row = new List<double>();
                for (int j = 0; j < cols; j++)
                {
                    row.Add(matrix[i, j]);
                }
                pythonMatrix.Add(row);
            }
            
            using var pyMatrix = PyObject.From(pythonMatrix);
            return arrayFunc.Call(pyMatrix);
        }
    }

    public static double[,] FromNumpyArray(PyObject numpyArray)
    {
        using (GIL.Acquire())
        {
            // Convert numpy array to .NET 2D array
            using var tolistMethod = numpyArray.GetAttr("tolist");
            using var pythonList = tolistMethod.Call();
            
            var listOfLists = pythonList.As<List<List<double>>>();
            
            var rows = listOfLists.Count;
            var cols = listOfLists[0].Count;
            var result = new double[rows, cols];
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = listOfLists[i][j];
                }
            }
            
            return result;
        }
    }
}
```

### Error Handling and Diagnostics

Advanced error handling for manual integration:

```csharp
public class RobustPythonCaller : IDisposable
{
    private readonly PyObject module;
    private readonly ILogger logger;

    public RobustPythonCaller(IPythonEnvironment env, string moduleName, ILogger logger)
    {
        this.logger = logger;
        
        try
        {
            using (GIL.Acquire())
            {
                module = Import.ImportModule(moduleName);
                logger.LogInformation("Successfully imported module: {ModuleName}", moduleName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import module: {ModuleName}", moduleName);
            throw;
        }
    }

    public T SafeCall<T>(string functionName, object[] args, T defaultValue = default(T))
    {
        try
        {
            using (GIL.Acquire())
            {
                logger.LogDebug("Calling function: {FunctionName} with {ArgCount} arguments", 
                    functionName, args.Length);

                if (!module.HasAttr(functionName))
                {
                    logger.LogWarning("Function {FunctionName} not found in module", functionName);
                    return defaultValue;
                }

                using var function = module.GetAttr(functionName);
                var pyArgs = args.Select(PyObject.From).ToArray();
                
                try
                {
                    using var result = function.Call(pyArgs);
                    var converted = result.As<T>();
                    
                    logger.LogDebug("Function {FunctionName} completed successfully", functionName);
                    return converted;
                }
                finally
                {
                    // Dispose all arguments
                    foreach (var arg in pyArgs)
                    {
                        arg.Dispose();
                    }
                }
            }
        }
        catch (PythonInvocationException ex)
        {
            logger.LogError(ex, "Python function {FunctionName} raised exception: {PythonException}", 
                functionName, ex.PythonExceptionType);
            return defaultValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error calling function: {FunctionName}", functionName);
            return defaultValue;
        }
    }

    public void Dispose()
    {
        try
        {
            module?.Dispose();
            logger.LogInformation("Module disposed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing module");
        }
    }
}
```

### Working with Python Classes

Manual integration with Python classes:

```csharp
public class PythonClassWrapper : IDisposable
{
    private readonly PyObject pythonClass;
    private readonly PyObject instance;

    public PythonClassWrapper(IPythonEnvironment env, string moduleName, string className, params object[] constructorArgs)
    {
        using (GIL.Acquire())
        {
            var module = Import.ImportModule(moduleName);
            pythonClass = module.GetAttr(className);
            
            var pyArgs = constructorArgs.Select(PyObject.From).ToArray();
            try
            {
                instance = pythonClass.Call(pyArgs);
            }
            finally
            {
                foreach (var arg in pyArgs)
                {
                    arg.Dispose();
                }
            }
        }
    }

    public T CallMethod<T>(string methodName, params object[] args)
    {
        using (GIL.Acquire())
        {
            using var method = instance.GetAttr(methodName);
            var pyArgs = args.Select(PyObject.From).ToArray();
            
            try
            {
                using var result = method.Call(pyArgs);
                return result.As<T>();
            }
            finally
            {
                foreach (var arg in pyArgs)
                {
                    arg.Dispose();
                }
            }
        }
    }

    public T GetProperty<T>(string propertyName)
    {
        using (GIL.Acquire())
        {
            using var property = instance.GetAttr(propertyName);
            return property.As<T>();
        }
    }

    public void SetProperty<T>(string propertyName, T value)
    {
        using (GIL.Acquire())
        {
            using var pyValue = PyObject.From(value);
            instance.SetAttr(propertyName, pyValue);
        }
    }

    public void Dispose()
    {
        instance?.Dispose();
        pythonClass?.Dispose();
    }
}
```

### Performance Optimization

Optimize manual integration for performance:

```csharp
public class OptimizedPythonCaller : IDisposable
{
    private readonly PyObject module;
    private readonly Dictionary<string, PyObject> cachedFunctions;
    private readonly ObjectPool<List<PyObject>> argsPool;

    public OptimizedPythonCaller(IPythonEnvironment env, string moduleName)
    {
        cachedFunctions = new Dictionary<string, PyObject>();
        argsPool = new ObjectPool<List<PyObject>>(() => new List<PyObject>());
        
        using (GIL.Acquire())
        {
            module = Import.ImportModule(moduleName);
        }
    }

    public T CallCached<T>(string functionName, params object[] args)
    {
        using (GIL.Acquire())
        {
            // Cache function references to avoid repeated attribute lookups
            if (!cachedFunctions.TryGetValue(functionName, out var function))
            {
                function = module.GetAttr(functionName);
                cachedFunctions[functionName] = function;
            }

            // Use object pool for arguments to reduce allocations
            var pyArgs = argsPool.Get();
            try
            {
                pyArgs.Clear();
                foreach (var arg in args)
                {
                    pyArgs.Add(PyObject.From(arg));
                }

                using var result = function.Call(pyArgs.ToArray());
                return result.As<T>();
            }
            finally
            {
                // Dispose arguments and return list to pool
                foreach (var arg in pyArgs)
                {
                    arg.Dispose();
                }
                argsPool.Return(pyArgs);
            }
        }
    }

    public void Dispose()
    {
        foreach (var function in cachedFunctions.Values)
        {
            function.Dispose();
        }
        module?.Dispose();
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
  <AdditionalFiles Include="*.py" />
</ItemGroup>
-->
```

### Option 2: Disable via MSBuild Property

Alternatively, you can disable the Source Generator by setting the `DisableCSnakesRuntimeSourceGenerator` property in the project file:

```xml
<PropertyGroup>
  <DisableCSnakesRuntimeSourceGenerator>true</DisableCSnakesRuntimeSourceGenerator>
</PropertyGroup>
```

## Best Practices for Manual Integration

### 1. Always Use GIL Protection

```csharp
// ✅ Good - GIL is acquired
using (GIL.Acquire())
{
    var result = function.Call(args);
    return result.As<string>();
}

// ❌ Bad - No GIL protection
var result = function.Call(args);  // This will crash!
```

### 2. Proper Resource Disposal

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

### 3. Error Handling

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

### 4. Module Caching

```csharp
// Cache modules at the class level
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
