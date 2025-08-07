# API Reference

This section provides detailed API reference for CSnakes types, methods, and interfaces.

## Core Interfaces

### IPythonEnvironment

The main interface for interacting with Python code from C#.

```csharp
public interface IPythonEnvironment
{
    // Generated module accessors (created by source generator)
    ModuleWrapper ModuleName();
    
    // Execute Python code directly
    PyObject ExecuteExpression(string expression);
    PyObject ExecuteExpression(string expression, Dictionary<string, PyObject> locals);
    PyObject ExecuteExpression(string expression, Dictionary<string, PyObject> locals, Dictionary<string, PyObject> globals);
    
    void Execute(string code);
    void Execute(string code, Dictionary<string, PyObject> locals);
    void Execute(string code, Dictionary<string, PyObject> locals, Dictionary<string, PyObject> globals);
}
```

### IPythonEnvironmentBuilder

Interface for configuring Python environments.

```csharp
public interface IPythonEnvironmentBuilder
{
    IPythonEnvironmentBuilder WithHome(string home);
    IPythonEnvironmentBuilder WithVirtualEnvironment(string path);
    IPythonEnvironmentBuilder WithCondaEnvironment(string environmentName);
    IPythonEnvironmentBuilder WithPipInstaller();
    
    // Locators
    IPythonEnvironment FromRedistributable(string version = "3.12", bool debug = false, bool freeThreaded = false);
    IPythonEnvironment FromEnvironmentVariable(string variableName, string version);
    IPythonEnvironment FromFolder(string path, string version);
    IPythonEnvironment FromSource(string path, string version, bool debug = false, bool freeThreaded = false);
    IPythonEnvironment FromMacOSInstaller(string version);
    IPythonEnvironment FromWindowsInstaller(string version);
    IPythonEnvironment FromWindowsStore(string version);
    IPythonEnvironment FromNuGet(string version);
    IPythonEnvironment FromConda(string condaPath);
}
```

## Python Object Types

### PyObject

Base class for all Python objects in C#.

```csharp
public class PyObject : IDisposable
{
    // Static factory methods
    public static PyObject From<T>(T value);
    public static PyObject None { get; }
    
    // Type checking
    public bool IsNone();
    public bool HasAttribute(string name);
    public Type GetPythonType();
    
    // Attribute access
    public PyObject GetAttribute(string name);
    public void SetAttribute(string name, PyObject value);
    
    // Conversion
    public T As<T>();
    public bool TryConvert<T>(out T result);
    
    // Comparison
    public bool Is(PyObject other);
    public bool Equals(PyObject other);
    public bool NotEquals(PyObject other);
    
    // String representation
    public override string ToString();
    
    // Disposal
    public void Dispose();
}
```

### IGeneratorIterator<TYield, TSend, TReturn>

Interface for Python generators.

```csharp
public interface IGeneratorIterator<out TYield, in TSend, out TReturn> : 
    IEnumerable<TYield>, IEnumerator<TYield>, IDisposable
{
    // Send values to generator
    TYield Send(TSend value);
    
    // Get return value (available after iteration completes)
    TReturn ReturnValue { get; }
    
    // Standard enumerator methods
    TYield Current { get; }
    bool MoveNext();
    void Reset();
    
    // Enumerable
    IEnumerator<TYield> GetEnumerator();
}
```

### IPyBuffer

Interface for Python buffer protocol objects (NumPy arrays, bytes, etc.).

```csharp
public interface IPyBuffer : IDisposable
{
    // Buffer properties
    IntPtr Buffer { get; }
    int Length { get; }
    bool IsReadOnly { get; }
    string Format { get; }
    int ItemSize { get; }
    int[] Shape { get; }
    int[] Strides { get; }
    
    // Conversion methods
    Span<T> AsSpan<T>() where T : unmanaged;
    ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged;
    T[] ToArray<T>() where T : unmanaged;
    
    // 2D operations (for matrices)
    Span2D<T> AsSpan2D<T>() where T : unmanaged;
    ReadOnlySpan2D<T> AsReadOnlySpan2D<T>() where T : unmanaged;
}
```

## Exception Types

### PythonInvocationException

Thrown when Python code raises an exception.

```csharp
public class PythonInvocationException : Exception
{
    public string PythonExceptionType { get; }
    public string PythonStackTrace { get; }
    public Dictionary<string, PyObject> PythonLocals { get; }
    public Dictionary<string, PyObject> PythonGlobals { get; }
    
    public PythonInvocationException(string message, string pythonExceptionType, 
        string pythonStackTrace, Exception innerException);
}
```

### PythonRuntimeException

Thrown when there are issues with the Python runtime itself.

```csharp
public class PythonRuntimeException : Exception
{
    public PythonRuntimeException(string message);
    public PythonRuntimeException(string message, Exception innerException);
}
```

### PythonStopIterationException

Thrown when a generator reaches the end of iteration.

```csharp
public class PythonStopIterationException : Exception
{
    public PyObject ReturnValue { get; }
    
    public PythonStopIterationException(PyObject returnValue);
}
```

## Configuration Types

### PythonEnvironmentOptions

Configuration options for Python environment setup.

```csharp
public class PythonEnvironmentOptions
{
    public string Home { get; set; }
    public string VirtualEnvironmentPath { get; set; }
    public string CondaEnvironmentName { get; set; }
    public bool UsePipInstaller { get; set; }
    public string PythonVersion { get; set; }
    public bool EnableDebugging { get; set; }
    public bool EnableFreeThreading { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; }
}
```

## Extension Methods

### ServiceCollectionExtensions

Extension methods for dependency injection setup.

```csharp
public static class ServiceCollectionExtensions
{
    public static IPythonEnvironmentBuilder WithPython(this IServiceCollection services);
    public static IServiceCollection AddPythonEnvironment(this IServiceCollection services, 
        Action<PythonEnvironmentOptions> configure);
}
```

### PyObjectExtensions

Extension methods for PyObject conversion and manipulation.

```csharp
public static class PyObjectExtensions
{
    public static T ConvertTo<T>(this PyObject pyObject);
    public static bool TryConvertTo<T>(this PyObject pyObject, out T result);
    public static Dictionary<string, object> ToDictionary(this PyObject pyObject);
    public static List<object> ToList(this PyObject pyObject);
}
```

## Type Conversion Reference

### Automatic Type Conversions

| C# Type | Python Type | Notes |
|---------|-------------|-------|
| `bool` | `bool` | Direct conversion |
| `long` | `int` | Python int can be arbitrarily large |
| `double` | `float` | IEEE 754 double precision |
| `string` | `str` | UTF-8 encoding |
| `byte[]` | `bytes` | Direct memory mapping |
| `IReadOnlyList<T>` | `list[T]` | Immutable view of Python list |
| `IReadOnlyDictionary<K,V>` | `dict[K,V]` | Immutable view of Python dict |
| `(T1, T2, ...)` | `tuple[T1, T2, ...]` | Value tuples |
| `T?` | `T \| None` | Nullable types |

### Manual Type Conversions

```csharp
// Converting PyObject to specific types
PyObject pyObj = env.SomeFunction();

// Direct conversion (throws if type doesn't match)
string str = pyObj.As<string>();
long number = pyObj.As<long>();
bool flag = pyObj.As<bool>();

// Safe conversion (returns false if conversion fails)
if (pyObj.TryConvert<Dictionary<string, object>>(out var dict))
{
    // Use dict
}

// Convert to .NET collection types
var list = pyObj.As<IReadOnlyList<object>>();
var dictionary = pyObj.As<IReadOnlyDictionary<string, object>>();

// Convert from .NET to PyObject
PyObject pyStr = PyObject.From("Hello");
PyObject pyNum = PyObject.From(42);
PyObject pyList = PyObject.From(new[] { 1, 2, 3 });
```

## Generated Code Reference

### Module Wrapper Classes

CSnakes generates wrapper classes for each Python module:

```csharp
// For a Python file named 'data_processor.py'
public class DataProcessorModule
{
    // For Python function: def process_data(items: list[str]) -> list[str]
    public IReadOnlyList<string> ProcessData(IReadOnlyList<string> items);
    
    // For Python function: def get_stats() -> dict[str, float]
    public IReadOnlyDictionary<string, double> GetStats();
    
    // For Python function: def async_operation() -> Coroutine[Any, Any, str]
    public Task<string> AsyncOperation();
}
```

### Naming Conventions

| Python | C# |
|--------|-----|
| `snake_case` | `PascalCase` |
| `my_function` | `MyFunction` |
| `calculate_average` | `CalculateAverage` |
| `get_user_data` | `GetUserData` |

### Async Function Generation

```python
# Python async function
async def fetch_data(url: str) -> dict[str, any]:
    # Implementation
    pass
```

```csharp
// Generated C# method
public Task<IReadOnlyDictionary<string, object>> FetchData(string url);
```

## Best Practices for API Usage

### Resource Management

```csharp
// ✅ Good - Proper disposal
using var result = env.SomeFunction();
var value = result.As<string>();

// ✅ Good - Using block
using (var pyObj = env.GetObject())
{
    // Use pyObj
} // Automatically disposed

// ❌ Bad - No disposal
var result = env.SomeFunction();
var value = result.As<string>();
// Memory leak - result not disposed
```

### Error Handling

```csharp
// ✅ Good - Comprehensive error handling
try
{
    var result = env.RiskyFunction();
    return result.As<string>();
}
catch (PythonInvocationException ex)
{
    // Handle Python-specific errors
    logger.LogError("Python error: {Type} - {Message}", 
        ex.PythonExceptionType, ex.Message);
    throw new ApplicationException($"Processing failed: {ex.Message}", ex);
}
catch (InvalidCastException ex)
{
    // Handle type conversion errors
    logger.LogError("Type conversion failed: {Message}", ex.Message);
    throw new ApplicationException("Invalid response format", ex);
}
```

### Performance Optimization

```csharp
// ✅ Good - Reuse module instances
private readonly MyModuleWrapper _module;

public MyService(IPythonEnvironment python)
{
    _module = python.MyModule(); // Create once
}

public string ProcessData(string input)
{
    return _module.ProcessString(input); // Reuse instance
}

// ❌ Bad - Create module instance each time
public string ProcessData(IPythonEnvironment python, string input)
{
    var module = python.MyModule(); // Creates new instance each time
    return module.ProcessString(input);
}
```

## Next Steps

- [Learn about type mapping](type-mapping.md)
- [Explore configuration options](configuration.md)
- [Review error handling](errors.md)
