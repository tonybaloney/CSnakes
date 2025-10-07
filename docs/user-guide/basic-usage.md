# Basic Usage

This guide covers the fundamental concepts and patterns for using CSnakes in your .NET applications.

## Basic Project Setup

### 1. Create Python Module

Create a Python file (e.g., `math_utils.py`) with type-annotated functions:

```python
def add(a: int, b: int) -> int:
    """Add two integers."""
    return a + b

def divide(a: float, b: float) -> float:
    """Divide two numbers."""
    if b == 0:
        raise ValueError("Cannot divide by zero")
    return a / b

def get_info() -> dict[str, str]:
    """Return system information."""
    import platform
    return {
        "system": platform.system(),
        "version": platform.version(),
        "machine": platform.machine()
    }
```

### 2. Configure Project File

Add the Python file to your `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="2.*-*" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include="math_utils.py" SourceItemType="Python">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>
</Project>
```

### 3. Initialize Python Environment

```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable();

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();
```

### 4. Call Python Functions

```csharp
// Get the generated module wrapper
var mathModule = env.MathUtils();

// Call Python functions
var sum = mathModule.Add(5, 3);
Console.WriteLine($"5 + 3 = {sum}"); // Output: 5 + 3 = 8

var quotient = mathModule.Divide(10.0, 3.0);
Console.WriteLine($"10 / 3 = {quotient:F2}"); // Output: 10 / 3 = 3.33

var info = mathModule.GetInfo();
foreach (var (key, value) in info)
{
    Console.WriteLine($"{key}: {value}");
}
```

## Naming Conventions

CSnakes automatically converts Python naming conventions to C# conventions for function names:

| Python | C# |
|--------|-----|
| `my_function` | `MyFunction` |
| `calculate_average` | `CalculateAverage` |
| `get_user_info` | `GetUserInfo` |

Function argument names are converted to C# conventions in lower case:

| Python | C# |
|--------|-----|
| `my_argument` | `myArgument` |
| `arg1` | `arg1` |

## Module Access

Python modules are accessed through the environment using the module's filename:

```python
# file: data_processor.py
def process_data(data: list[int]) -> list[int]:
    return [x * 2 for x in data]
```

```csharp
// Access the module
var processor = env.DataProcessor();
var result = processor.ProcessData(new[] { 1, 2, 3, 4 });
// result: [2, 4, 6, 8]
```

## Error Handling

Python exceptions are automatically converted to `PythonInvocationException`:

```csharp
try
{
    var result = mathModule.Divide(10.0, 0.0);
}
catch (PythonInvocationException ex)
{
    Console.WriteLine($"Python error: {ex.Message}");
    Console.WriteLine($"Python type: {ex.PythonExceptionType}");
    Console.WriteLine($"Stack trace: {ex.PythonStackTrace}");
}
```

## Working with Complex Types

### Lists and Collections

```python
def process_numbers(numbers: list[int]) -> list[int]:
    return [n * 2 for n in numbers if n > 0]

def get_user_names() -> list[str]:
    return ["Alice", "Bob", "Charlie"]
```

```csharp
var numbers = new[] { -1, 2, -3, 4, 5 };
var processed = mathModule.ProcessNumbers(numbers);
// Result: [4, 8, 10]

var names = mathModule.GetUserNames();
foreach (var name in names)
{
    Console.WriteLine(name);
}
```

### Dictionaries

```python
def create_user(name: str, age: int) -> dict[str, str | int]:
    return {"name": name, "age": age, "id": hash(name)}
```

```csharp
var user = mathModule.CreateUser("Alice", 30);
Console.WriteLine($"Name: {user["name"]}");
Console.WriteLine($"Age: {user["age"]}");
```

### Tuples

```python
def get_coordinates() -> tuple[float, float]:
    return (12.34, 56.78)

def parse_name(full_name: str) -> tuple[str, str]:
    parts = full_name.split(" ", 1)
    return (parts[0], parts[1] if len(parts) > 1 else "")
```

```csharp
var coordinates = mathModule.GetCoordinates();
Console.WriteLine($"X: {coordinates.Item1}, Y: {coordinates.Item2}");

var (firstName, lastName) = mathModule.ParseName("John Doe");
Console.WriteLine($"First: {firstName}, Last: {lastName}");
```

## Optional Parameters

Python default values are preserved in the generated C# methods:

```python
def greet(name: str, prefix: str = "Hello", suffix: str = "!") -> str:
    return f"{prefix}, {name}{suffix}"
```

```csharp
// All these calls are valid:
var greeting1 = mathModule.Greet("Alice");                    // "Hello, Alice!"
var greeting2 = mathModule.Greet("Bob", "Hi");                // "Hi, Bob!"
var greeting3 = mathModule.Greet("Charlie", "Hey", "!!!");    // "Hey, Charlie!!!"
```

## Next Steps

- [Learn about the type system](type-system.md)
- [Set up virtual environments](environments.md)
- [Work with NumPy arrays and buffers](buffers.md)
