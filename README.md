# CSnakes - a tool for embedding Python into .NET projects

> [!WARNING]  
> This project is in prototype stage and the API is subject to change. 

[Documentation](https://tonybaloney.github.io/CSnakes/)

[![NuGet Version](https://img.shields.io/nuget/v/CSnakes.Runtime?label=CSnakes.Runtime)](https://www.nuget.org/packages/CSnakes.Runtime)

<img src="docs/res/logo.jpeg" alt="drawing" width="200"/> 

CSnakes is a .NET Source Generator and Runtime that you can use to embed Python code and libraries into your .NET Solution without the need for REST, HTTP, or Microservices.

![image](https://github.com/tonybaloney/CSnakes/assets/1532417/39ca2f2a-416b-447a-a237-59e9613a4990)

## Features

- Supports .NET Standard 8-9
- Supports Python 3.9-3.13
- [Supports Virtual Environments and C-Extensions](https://tonybaloney.github.io/CSnakes/getting-started/#using-virtual-environments)
- Supports Windows, macOS, and Linux
- [Tight integration between NumPy ndarrays and Spans, 2D Spans and TensorSpans (.NET 9)](https://tonybaloney.github.io/CSnakes/buffers/)
- Uses Python's C-API for fast invocation of Python code directly in the .NET process
- Uses Python type hinting to generate function signatures with .NET native types
- Supports CPython 3.13 "free-threading" mode
- Supports nested sequence and mapping types (`tuple`, `dict`, `list`)
- Supports default values

## Examples

Given the following Python file called `example.py`

```python

def hello_world(name: str, age: int) -> str:
  return f"Hello {name}, you must be {age} years old!"
```

CSnakes will generate a static .NET class called `Example` with the function:

```csharp
public class Example {
  public static string HelloWorld(string name, long age) {
    ...
  }
}
```

When called, `HelloWorld()` will invoke the Python function from `example.py` using Python's C-API and return native .NET types.

## FAQ

See the [FAQ](docs/faq.md) for more information.
