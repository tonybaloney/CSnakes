# CSnakes - a tool for embedding Python code into .NET projects

[![NuGet Version](https://img.shields.io/nuget/v/CSnakes)](https://www.nuget.org/packages/CSnakes)

<img src="res/logo.jpeg" alt="drawing" width="200"/> 

CSnakes is a .NET Source Generator and Runtime that you can use to embed Python code and libraries into your .NET Solution without the need for REST, HTTP, or Microservices.

![image](https://github.com/tonybaloney/PythonCodeGen/assets/1532417/39ca2f2a-416b-447a-a237-59e9613a4990)

## Features

- .NET Standard 2.0 (.NET 6+)
- Supports up to Python 3.12
- Supports Virtual Environments and C-Extensions
- Supports Windows, macOS, and Linux
- Uses Python's C-API for fast invocation of Python code directly in the .NET process
- Uses Python type hinting to generate function signatures with .NET native types
- Supports nested sequence and mapping types (`tuple`, `dict`, `list`)
- Supports default values

## Example

CSnakes will generate a C#.NET class for any Python file in a project that is tagged as CSharp Analyzer Additional File (see [Getting Started](getting-started.md)).
All functions in that class with type annotations will be reflected to callable C# methods and an environment builder added to that module.

## Supported Types

CSnakes supports the following typed scenarios:

| Python type annotation | Reflected C# Type |
|------------------------|-------------------|
| `int`                  | `long`            |
| `float`                | `double`          |
| `str`                  | `string`          |
| `bool`                 | `bool`            |
| `list[T]`              | `IEnumerable<T>`  |
| `dict[K, V]`           | `IReadOnlyDictionary<K, V>` |
| `tuple[T1, T2, ...]`   | `(T1, T2, ...)`   |

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
