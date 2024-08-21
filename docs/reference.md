# Reference

## Supported Types

CSnakes supports the following typed scenarios:

| Python type annotation | Reflected C# Type |
|------------------------|-------------------|
| `int`                  | `long`            |
| `float`                | `double`          |
| `str`                  | `string`          |
| `bytes`                | `byte[]`          |
| `bool`                 | `bool`            |
| `list[T]`              | `IEnumerable<T>`  |
| `dict[K, V]`           | `IReadOnlyDictionary<K, V>` |
| `tuple[T1, T2, ...]`   | `(T1, T2, ...)`   |
| `typing.Sequence[T]`   | `IEnumerable<T>`  |
| `typing.Dict[K, V]`    | `IReadOnlyDictionary<K, V>` |
| `typing.Mapping[K, V]` | `IReadOnlyDictionary<K, V>` |
| `typing.Tuple[T1, T2, ...]` | `(T1, T2, ...)` |
| `typing.Optional[T]`   | `T?`              |
| `typing.Generator[TYield, TSend, TReturn]` | `IGeneratorIterator<TYield, TSend, TReturn>` [1](#generators) |
| `None` (Return)        | `void`            |


### Return types

The same type conversion applies for the return type of the Python function, with the additional feature that functions which explicitly return type `None` are declared as `void` in C#.

### Default values

CSnakes will use the default value for arguments of types `int`, `float`, `str`, and `bool` for the generated C# method. For example, the following Python code:

```python
def example(a: int = 123, b: bool = True, c: str = "hello", d: float = 1.23) -> None
  ...

```

Will generate the following C#.NET method signature:

```csharp
public void Example(long a = 123, bool b = true, string c = "hello", double d = 1.23)
```

1. CSnakes will treat `=None` default values as nullable arguments. The Python runtime will set the value of the parameter to the `None` value at execution.

## Python Locators

CSnakes uses a `PythonLocator` to find the Python runtime on the host machine. The `PythonLocator` is a service that is registered with the dependency injection container and is used to find the Python runtime on the host machine.

You can chain locators together to match use the first one that finds a Python runtime. This is a useful pattern for code that is designed to run on Windows, Linux, and MacOS.

### Environment Variable Locator

The `.FromEnvironmentVariable()` method allows you to specify an environment variable that contains the path to the Python runtime. This is useful for scenarios where the Python runtime is installed in a non-standard location or where the path to the Python runtime is not known at compile time.

This locator is also very useful for GitHub Actions `setup-python` actions, where the Python runtime is installed in a temporary location specified by the environment variable "`Python3_ROOT_DIR`":

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
```

### Folder Locator

The `.FromFolder()` method allows you to specify a folder that contains the Python runtime. This is useful for scenarios where the Python runtime is installed in a known location on the host machine.

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromFolder(@"C:\path\to\python\3.12", "3.12")
```

### Source Locator

The Source Locator is used to find a compiled Python runtime from source. This is useful for scenarios where you have compiled Python from source and want to use the compiled runtime with CSnakes.

It optionally takes a `bool` parameter to specify that the binary is debug mode and to enable free-threading mode in Python 3.13:

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromSource(@"C:\path\to\cpython\", "3.13", debug: true,  freeThreaded: true)
```

### MacOS Installer Locator

The MacOS Installer Locator is used to find the Python runtime on MacOS. This is useful for scenarios where you have installed Python from the official Python installer on MacOS from [python.org](https://www.python.org/downloads/).

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromMacOSInstaller("3.12")
```

### Windows Installer Locator

The Windows Installer Locator is used to find the Python runtime on Windows. This is useful for scenarios where you have installed Python from the official Python installer on Windows from [python.org](https://www.python.org/downloads/).

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromWindowsInstaller("3.12")
```

### Windows Store Locator

The Windows Store Locator is used to find the Python runtime on Windows from the Windows Store. This is useful for scenarios where you have installed Python from the Windows Store on Windows.

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromWindowsStore("3.12")
```

### Nuget Locator

The Nuget Locator is used to find the Python runtime from a Nuget package. This is useful for scenarios where you have installed Python from one of the Python Nuget packages found at [nuget.org](https://www.nuget.org/packages/python/).

These packages only bundle the Python runtime for Windows. You also need to specify the minor version of Python:

```csharp
...
var pythonBuilder = services.WithPython();
pythonBuilder.FromNuGet("3.12.4")
```

## Parallelism and concurrency

CSnakes is designed to be thread-safe and can be used in parallel execution scenarios. 

See [Advanced Usage](advanced.md) for more information on using CSnakes in a multi-threaded environment.

## Implementation details

CSnakes uses the Python C-API to invoke Python code from C#. The Python C-API is a low-level interface to the Python runtime that allows you to interact with Python objects and execute Python code from C.

CSnakes generates a C# class that handles the calls and conversions between Python and C#. The generated class is a wrapper around the Python C-API that allows you to call Python functions and methods from C#.

The generated class uses the `Python.Runtime` library to interact with the Python C-API. The `Python.Runtime` library is a C# wrapper around the Python C-API that provides a high-level interface to the Python runtime.

The `PyObject` type is a Managed Handle to a Python object. It is a reference to a Python object that is managed by the Python runtime. The `PyObject` type is used to pass Python objects around inside C#. All `PyObject` instances have been created with a strong reference in C#. They are automatically garbage collected when the last reference is released. These objects are also disposable, so you can release the reference manually if you need to:

```csharp
using CSnakes.Runtime.Python;

{
  using PyObject obj = env.MethodToCall();

  Console.WriteLine(obj.ToString());
}  // obj is disposed here
```

### GIL and multi-threading

CSnakes uses the Python Global Interpreter Lock (GIL) to ensure that only one thread can execute Python code at a time. This is necessary because the Python runtime is not thread-safe and can crash if multiple threads try to execute Python code simultaneously.

All public methods generate by CSnakes have a built-in GIL acquisition and release mechanism. This means that you can safely call Python functions from multiple threads without worrying about the GIL.

## Exceptions

CSnakes will raise a `PythonInvocationException` if an error occurs during the execution of the Python code. The `PythonInvocationException` class contains the error message from the Python interpreter as the `InnerException`.

If the annotations are incorrect and your Python code returns a different type to what CSnakes was expecting, an `InvalidCastException` will be thrown with the details of the source and destination types.

### Fetching stack traces

You can fetch the Python stack trace as well as the Globals and Locals of the top frame by getting the `InnerException` attribute of the raised `PythonInvocationException`:

```csharp
try
{
  env.MethodToCall();
}
catch (PythonInvocationException ex)
{
  Console.WriteLine(ex.PythonExceptionType); // E.g. ValueError
  Console.WriteLine(ex.InnerException.PythonStackTrace); // IEnumerable<string> with the complete stack trace
  Console.WriteLine(ex.InnerException.Data["locals"]); // Dictionary <string, PyObject>
  Console.WriteLine(ex.InnerException.Data["globals"]); // Dictionary <string, PyObject>
}
```

## Generators

CSnakes supports [Python generators](https://docs.python.org/3/library/typing.html#typing.Generator) using the `typing.Generator` type annotation. The `typing.Generator` type annotation is used to specify a generator function that yields values of a specific type.

CSnakes will convert a Python Generator to a CLR type in the `CSnakes.Runtime.Python` namespace that implements the `IGeneratorIterator` interface.

The `IGeneratorIterator` implements both `IEnumerable<T>` and `IEnumerator<T>` interfaces, so you can use it in `foreach` loops and LINQ queries.

For example the Python function:

```python
from typing import Generator


def example_generator(length: int) -> Generator[str, int, bool]:
    for i in range(length):
        x = yield f"Item {i}"
        if x:
            yield f"Received {x}"

    return True
```

Will return a `IGeneratorIterator<string, long, bool>`. You can use this in a C# foreach loop:

```csharp
var generator = env.ExampleGenerator(5);

foreach (string item in generator)
{
	Console.WriteLine(item);
}
```

`IGeneratorIterator` also implements a `Send` method that allows you to send values back into the generator.

The type of `.Send` is the `TSend` type parameter of the `Generator` type annotation and returns `TYield`. In the example above, the `TSend` type is `long`, so you can send a `long` value back into the generator:

```csharp
var generator = env.ExampleGenerator(5);
string nextValue= generator.Send(10);
```