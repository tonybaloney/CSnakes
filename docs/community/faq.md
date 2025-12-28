# Frequently Asked Questions

## What is the purpose of this project?

CSnakes is a .NET Source Generator and Runtime that you can use to embed **Python** code and libraries into your **C#.NET Solution** at a performant, low-level without the need for REST, HTTP, or Microservices.

## How does it work?

CSnakes uses the Python C-API to invoke Python code directly from the .NET process. It generates C# code from Python files that are tagged as CSharp Analyzer Additional Files. The generated code includes the function signatures and type conversions based on the type hints in the Python code.

See the [Getting Started](../getting-started/quick-start.md) guide for more information.

## What are the benefits of using CSnakes?

- Uses native Python type hinting standards to produce clean, readable C# code with minimal boiler plate!
- Integration between .NET and Python is done at the C-API, meaning strong compatibility between Python versions 3.8-3.12 and .NET 6-8.
- Integration is low-level and high-performance.
- CSnakes uses the CPython C-API and is compatible with all Python extensions.
- Invocation of Python code and libraries is in the same process as .NET

## How does this compare to IronPython?

IronPython is a .NET implementation of Python that runs on the .NET runtime. CSnakes is a tool that allows you to embed Python code and libraries into your C#.NET solution. CSnakes uses the Python C-API to invoke Python code directly from the .NET process, whereas IronPython is a separate implementation of Python that runs on the .NET runtime.

## How does this compare to Python.NET?

There are some similarities, such as Python.NET has a wrapper around the Python C-API. However, CSnakes comes with a Source Generator to create an interop layer automatically between the two platforms and handle the type conversions based on the type hints in the Python code.

## Do I need to use the source generator?

No. You can call Python code without the Source Generator, but you will need to write the boilerplate code yourself. See the [Manual Integration](../advanced/manual-integration.md) guide for more information.

## My Python codes uses libraries which have C extensions, will this work?

Yes. CSnakes supports C-Extensions and Virtual Environments. You can use the `WithPython` extension method to specify the path to your Python modules and the `.WithVirtualEnvironment()` method to load the Python runtime from a virtual environment.

## Does this mean I need to type annotate all my Python code?

No. Just the functions you want to call from C#. The Source Generator will generate the C# code based on the type hints in the Python code. Only the Python files marked in the C# project as CSharp Analyzer Additional Files will be processed.

## I heard that Python is removing the GIL, does CSnakes handle threads that way?

CSnakes supports free-threading mode, but it is disabled by default. You can use the `SourceLocator` to find a compiled Python runtime with free-threading enabled. See the [Free-Threading](../advanced/free-threading.md) guide for more information.
