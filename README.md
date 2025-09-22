# CSnakes - a tool for embedding Python code into .NET projects

[Documentation](https://tonybaloney.github.io/CSnakes/)

[![NuGet Version](https://img.shields.io/nuget/v/CSnakes.Runtime?label=CSnakes.Runtime)](https://www.nuget.org/packages/CSnakes.Runtime)

<img src="docs/res/logo.jpeg" alt="drawing" width="200"/> 

CSnakes is a .NET Source Generator and Runtime that you can use to embed **Python** code and libraries into your **C#.NET Solution** at a performant, low-level without the need for REST, HTTP, or Microservices.

Check out the [getting started](https://tonybaloney.github.io/CSnakes/latest/getting-started/quick-start/) guide or check out the [demo solution](https://github.com/tonybaloney/CSnakes/tree/main/samples) to see more.

## Features

- 🤖 Supports .NET 8 and 9  
- 🐍 Supports Python 3.9-3.13  
- 📦 [Supports Virtual Environments and C-Extensions](https://tonybaloney.github.io/CSnakes/latest/user-guide/environments/)  
- 💻 Supports Windows, macOS, and Linux  
- 🧮 [Tight integration between NumPy ndarrays and Spans, 2D Spans and TensorSpans (.NET 9)](https://tonybaloney.github.io/CSnakes/latest/user-guide/buffers/)  
- ⚡ Uses Python's C-API for fast invocation of Python code directly in the .NET process  
- 🧠 Uses Python type hinting to generate function signatures with .NET native types  
- 🧵 Supports [CPython 3.13 "free-threading" mode](https://tonybaloney.github.io/CSnakes/latest/advanced/free-threading/)  
- 🧩 Supports nested sequence and mapping types (`tuple`, `dict`, `list`)  
- 🏷️ Supports default values  
- 🔥 Supports [Hot Reload](https://tonybaloney.github.io/CSnakes/latest/advanced/hot-reload/) of Python code in Visual Studio and supported IDEs  
- 🚀 Supports [UV](https://tonybaloney.github.io/CSnakes/latest/user-guide/environments/#installing-dependencies-with-uv) for fast installation of Python packages and dependencies  

## Benefits

- Uses native Python type hinting standards to produce clean, readable C# code with minimal boiler plate!
- Integration between .NET and Python is done at the C-API, meaning strong compatibility between Python versions 3.8-3.13 and .NET 8-9.
- Integration is low-level and high-performance.
- CSnakes uses the CPython C-API and is compatible with all Python extensions.
- Invocation of Python code and libraries is in the same process as .NET

<br />

[![CSnakes Demo Video](https://img.youtube.com/vi/fDbCqalegNU/0.jpg)](https://www.youtube.com/watch?v=fDbCqalegNU)

*Click to watch the CSnakes demo video on YouTube*

<br />

## Example

CSnakes will generate a C#.NET class for any Python file in a project that is tagged as CSharp Analyzer Additional File (see [Getting Started](https://tonybaloney.github.io/CSnakes/latest/getting-started/quick-start/)).
All functions in that class with type annotations will be reflected to callable C# methods and an environment builder added to that module.

![System diagram](docs/res/architecture_simple.png)

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

See the [FAQ](https://tonybaloney.github.io/CSnakes/latest/community/faq/) for more information.

## Development / Contributing

For local development clone repo with:

```shell
git clone --recurse-submodules https://github.com/tonybaloney/CSnakes.git
```
