# Your First Example

Let's create a simple example that demonstrates calling Python code from C#.

## Step 1: Create the Python File

Create a Python file called `demo.py` in your project with the following content:

```python
def hello_world(name: str) -> str:
    return f"Hello, {name}!"

def add_numbers(a: int, b: int) -> int:
    return a + b

def calculate_average(numbers: list[float]) -> float:
    if not numbers:
        return 0.0
    return sum(numbers) / len(numbers)
```

## Step 2: Configure the Project File

Mark the Python file as an "Additional File" in your `.csproj` file:

```xml
<ItemGroup>
    <AdditionalFiles Include="demo.py">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
</ItemGroup>
```

MSBuild supports globbing, so you can also mark all Python files in a directory
as additional files:

```xml
<ItemGroup>
    <AdditionalFiles Include="*.py">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
</ItemGroup>
```

Alternatively, you can set CSnakes to parse all `*.py` files by default, by
setting the `DefaultPythonItems` property to `true` in the project file:

```xml
<PropertyGroup>
    <DefaultPythonItems>true</DefaultPythonItems>
</PropertyGroup>
```

## Step 3: Write the C# Code

```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var home = Path.Join(Environment.CurrentDirectory, ".");

builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable(); // Downloads Python automatically

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

// Get the Python module
var module = env.Demo();

// Call Python functions
var greeting = module.HelloWorld("Alice");
Console.WriteLine(greeting); // Output: Hello, Alice!

var sum = module.AddNumbers(5, 3);
Console.WriteLine($"5 + 3 = {sum}"); // Output: 5 + 3 = 8

var numbers = new[] { 1.5, 2.5, 3.5, 4.5 };
var average = module.CalculateAverage(numbers);
Console.WriteLine($"Average: {average}"); // Output: Average: 2.75
```

## Step 4: Build and Run

Build your project:

```bash
dotnet build
```

Run your application:

```bash
dotnet run
```

## What's Happening?

1. **Type Annotations**: CSnakes uses Python type annotations to generate C# method signatures
2. **Source Generation**: The CSnakes source generator creates C# wrapper methods for your Python functions
3. **Method Names**: Python function names are converted to PascalCase (e.g., `hello_world` becomes `HelloWorld`)
4. **Type Conversion**: Python types are automatically converted to their C# equivalents

## Generated C# Signatures

From our Python functions, CSnakes generates these C# method signatures:

```csharp
public string HelloWorld(string name);
public long AddNumbers(long a, long b);
public double CalculateAverage(IReadOnlyList<double> numbers);
```

## Error Handling

If a Python function raises an exception, it will be caught and re-thrown as a `PythonInvocationException`:

```csharp
try
{
    var result = module.SomeFunction();
}
catch (PythonInvocationException ex)
{
    Console.WriteLine($"Python error: {ex.Message}");
}
```

## Next Steps

- [Learn about supported types](../user-guide/type-system.md)
- [Explore more examples](../examples/sample-projects.md)
- [Set up virtual environments](../user-guide/environments.md)
