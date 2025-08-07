# Hot Reload Support

CSnakes supports [hot reload](https://learn.microsoft.com/visualstudio/debugger/hot-reload?view=vs-2022) of Python code in Visual Studio and supported IDEs. This means that you can make changes to your Python code within the function body and see the changes reflected in your C# code without restarting the application.

## Overview

Hot reload functionality allows for rapid development iteration by automatically reloading Python modules when their source files change. This feature is particularly useful during development when you need to quickly test changes to Python logic without restarting your entire .NET application.

## How It Works

This feature is enabled in the generated classes in CSnakes. When you make changes to the Python code, the modules are reloaded in the .NET runtime and subsequent calls to the Python code will use the new code.

The hot reload mechanism:

1. **Monitors Python files** for changes during development
2. **Automatically reloads** modified modules
3. **Updates function bindings** to use the new code
4. **Preserves application state** in your .NET application

## Enabling Hot Reload

### Visual Studio 2022

To enable Hot Reload in Visual Studio 2022, see the [official documentation](https://learn.microsoft.com/visualstudio/debugger/hot-reload?view=vs-2022).

1. **Start debugging** your application (F5 or Debug > Start Debugging)
2. **Make changes** to your Python files
3. **Save the files** - changes should be applied automatically
4. **Verify changes** by calling the updated Python functions

### VS Code

Hot reload works with VS Code when using the C# extension and debugging:

1. **Set breakpoints** in your C# code
2. **Start debugging** (F5)
3. **Modify Python files** while debugging
4. **Save changes** - they will be reflected in subsequent calls

### Command Line

Hot reload also works when running your application with `dotnet run`:

```bash
dotnet run
# Application starts, make changes to Python files
# Changes are automatically picked up
```

## Example Usage

### Python Module (calculator.py)

```python
def add(a: int, b: int) -> int:
    return a + b

def multiply(a: int, b: int) -> int:
    return a * b

def describe_operation(operation: str) -> str:
    return f"Performing {operation} operation"
```

### C# Application

```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable();

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

var calc = env.Calculator();

while (true)
{
    Console.WriteLine($"5 + 3 = {calc.Add(5, 3)}");
    Console.WriteLine($"5 * 3 = {calc.Multiply(5, 3)}");
    Console.WriteLine(calc.DescribeOperation("addition"));
    
    Console.WriteLine("Press any key to test again (or Ctrl+C to exit)...");
    Console.ReadKey();
    Console.Clear();
}
```

### Hot Reload in Action

1. **Start the application** in debug mode
2. **Modify the Python function** while the app is running:

```python
def add(a: int, b: int) -> int:
    # Hot reload change - add logging
    result = a + b
    print(f"DEBUG: Adding {a} + {b} = {result}")
    return result

def describe_operation(operation: str) -> str:
    # Hot reload change - enhanced description
    return f"🔥 Hot reloaded: Performing {operation} operation with CSnakes!"
```

3. **Save the file** - changes are automatically applied
4. **Continue using the application** - new behavior is immediately available

## Supported Changes

Hot reload supports changes to the **function body** of Python functions:

### ✅ Supported Changes

- **Logic modifications** within function bodies
- **Adding/removing** local variables
- **Changing calculations** and algorithms
- **Modifying string literals** and constants
- **Adding/removing** print statements or logging
- **Changing loop logic** and conditionals
- **Importing additional modules** within functions

### Example of Supported Changes

```python
# Original function
def process_data(items: list[int]) -> list[int]:
    return [x * 2 for x in items]

# Hot reload - change algorithm
def process_data(items: list[int]) -> list[int]:
    # New logic with filtering and transformation
    filtered = [x for x in items if x > 0]
    result = [x * 3 + 1 for x in filtered]  # Changed formula
    print(f"Processed {len(filtered)} items")  # Added logging
    return result
```

## Limitations

Beyond the C# [limitations](https://learn.microsoft.com/visualstudio/debugger/supported-code-changes-csharp?view=vs-2022), Hot Reload does not support changes to the Python code which require additional changes to the C# interface:

### ❌ Unsupported Changes

- **Removing functions** - C# code still references them
- **Changing function signatures** - Parameter types or counts
- **Changing return types** - Would break C# type expectations
- **Changing parameter types** - Would cause type conversion errors
- **Changing function names** - C# bindings use the original names
- **Changing module names** - Module import references are cached
- **Adding new functions** - Not accessible without regenerating bindings

### Examples of Unsupported Changes

```python
# ❌ Can't change function signature during hot reload
# Original:
def calculate(a: int, b: int) -> int:
    return a + b

# This won't work in hot reload:
def calculate(a: int, b: int, c: int) -> int:  # Added parameter
    return a + b + c

# ❌ Can't change return type
# Original:
def get_result() -> int:
    return 42

# This won't work in hot reload:
def get_result() -> str:  # Changed return type
    return "42"
```

## Best Practices

### 1. Structure for Hot Reload

Design your Python code to maximize hot reload benefits:

```python
# Good structure for hot reload
def main_algorithm(data: list[int]) -> list[int]:
    # Main logic that might change frequently
    processed = process_step_1(data)
    processed = process_step_2(processed)
    processed = apply_business_rules(processed)
    return processed

def process_step_1(data: list[int]) -> list[int]:
    # Specific step that can be modified independently
    return [x * 2 for x in data]

def process_step_2(data: list[int]) -> list[int]:
    # Another step that can be hot reloaded
    return [x + 10 for x in data if x > 0]

def apply_business_rules(data: list[int]) -> list[int]:
    # Business logic that might change during development
    return sorted(data, reverse=True)
```

### 2. Use Configuration for Behavior Changes

Instead of changing function signatures, use configuration:

```python
# Configuration-driven approach
def process_data(data: list[int], config: dict[str, any] = None) -> list[int]:
    if config is None:
        config = {"multiplier": 2, "filter_threshold": 0, "sort_desc": True}
    
    # These values can be changed during hot reload
    multiplier = config.get("multiplier", 2)
    threshold = config.get("filter_threshold", 0)
    sort_desc = config.get("sort_desc", True)
    
    filtered = [x for x in data if x > threshold]
    processed = [x * multiplier for x in filtered]
    
    return sorted(processed, reverse=sort_desc)
```

### 3. Add Debugging Support

Include debugging features that can be toggled during hot reload:

```python
def analyze_data(data: list[float]) -> dict[str, float]:
    # Debug flag that can be changed during hot reload
    debug_mode = True  # Change this during development
    
    if debug_mode:
        print(f"Input data: {data[:5]}...")  # Show first 5 items
        print(f"Data length: {len(data)}")
    
    result = {
        "mean": sum(data) / len(data),
        "max": max(data),
        "min": min(data)
    }
    
    if debug_mode:
        print(f"Results: {result}")
    
    return result
```

### 4. Implement Feature Flags

Use feature flags for experimental features:

```python
def enhanced_processing(data: list[int]) -> list[int]:
    # Feature flags that can be toggled during development
    use_new_algorithm = True  # Toggle during hot reload
    enable_optimization = False  # Test optimizations
    verbose_logging = True  # Debug output
    
    if verbose_logging:
        print(f"Processing {len(data)} items")
    
    if use_new_algorithm:
        # New algorithm implementation
        result = [x ** 2 for x in data if x % 2 == 0]
    else:
        # Original algorithm
        result = [x * 2 for x in data]
    
    if enable_optimization:
        # Apply optimization (can be tested during hot reload)
        result = list(set(result))  # Remove duplicates
        result.sort()
    
    if verbose_logging:
        print(f"Produced {len(result)} results")
    
    return result
```

## Development Workflow

### Typical Hot Reload Workflow

1. **Start debugging** your .NET application
2. **Set breakpoints** at key points where Python functions are called
3. **Run the application** to the breakpoint
4. **Examine Python function behavior**
5. **Modify Python code** based on observations
6. **Save changes** (hot reload applies automatically)
7. **Continue execution** or restart from breakpoint
8. **Repeat** until desired behavior is achieved

### Example Development Session

```csharp
// C# debugging code
public void DevelopmentTestLoop()
{
    var processor = env.DataProcessor();
    var testData = new[] { 1, 2, 3, 4, 5, -1, -2, 0 };
    
    while (true)
    {
        Console.WriteLine("Testing current implementation:");
        
        try
        {
            var result = processor.ProcessData(testData.ToList());
            Console.WriteLine($"Result: [{string.Join(", ", result)}]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("Modify Python code and press any key to test again...");
        Console.ReadKey();
        Console.Clear();
    }
}
```

## Troubleshooting Hot Reload

### Common Issues

1. **Changes not reflected**
   - Ensure file is saved
   - Check that debugger is attached
   - Verify the file is in the correct location

2. **Application crashes after reload**
   - Check for syntax errors in Python code
   - Ensure return types haven't changed
   - Verify function signatures remain the same

3. **Performance degradation**
   - Hot reload may cause slight performance overhead
   - Disable in production environments

### Debugging Hot Reload Issues

```python
# Add diagnostics to your Python code
def debug_hot_reload_status() -> dict[str, any]:
    import sys
    import os
    from datetime import datetime
    
    return {
        "timestamp": datetime.now().isoformat(),
        "python_version": sys.version,
        "current_file": __file__,
        "file_modified": os.path.getmtime(__file__),
        "modules_loaded": list(sys.modules.keys())[-10:]  # Last 10 modules
    }
```

```csharp
// Check hot reload status from C#
var debugInfo = env.YourModule().DebugHotReloadStatus();
Console.WriteLine($"Python code timestamp: {debugInfo["timestamp"]}");
Console.WriteLine($"File last modified: {debugInfo["file_modified"]}");
```

## Production Considerations

### Disabling Hot Reload in Production

Hot reload is primarily a development feature. In production:

1. **Use release builds** which typically have hot reload disabled
2. **Deploy pre-compiled** Python bytecode if possible
3. **Monitor performance** to ensure hot reload overhead is not present

### Configuration

```csharp
// Production configuration
var builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Hot reload enabled in development
    builder.Services.WithPython()
        .WithHome(Environment.CurrentDirectory)
        .FromRedistributable();
}
else
{
    // Optimized for production
    builder.Services.WithPython()
        .WithHome("/app/python")  // Fixed path
        .FromRedistributable()
        .DisableHotReload();      // Hypothetical API
}
```

## Next Steps

- [Learn about signal handlers](signal-handlers.md)
- [Explore Native AOT support](native-aot.md)
- [Review troubleshooting guide](troubleshooting.md)
