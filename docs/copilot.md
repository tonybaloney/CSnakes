# AI Development Assistant Prompts for CSnakes

This page contains practical prompts designed to help you solve common CSnakes development challenges using AI development assistants like GitHub Copilot, ChatGPT, Claude, or other AI coding tools.

## Convert Python Script to CSnakes-ready Function

**Problem**: You have a standalone Python script and need to convert it into a function that CSnakes can generate C# bindings for.

**Copy this prompt into your AI assistant:**

```
Convert this Python script into a CSnakes-compatible function with proper type annotations.

Use these supported type mappings:
- int → long (C#)
- float → double (C#) 
- str → string (C#)
- bool → bool (C#)
- bytes → byte[] (C#)
- list[T] → IReadOnlyList<T> (C#)
- dict[K, V] → IReadOnlyDictionary<K, V> (C#)
- tuple[T1, T2, ...] → (T1, T2, ...) (C#)
- typing.Optional[T] or T | None → T? (C#)
- typing.Buffer → IPyBuffer (C#)
- None (return only) → void (C#)

Requirements:

1. Convert the standalone script into a function with parameters
2. Add proper type annotations to all parameters and return values
3. Use default parameter values for optional arguments (int, float, str, bool only)
4. Preserve the original logic and functionality
5. Add docstrings explaining what each function does
6. Handle any global variables by making them function parameters

Here's my Python script:
[PASTE YOUR SCRIPT HERE]
```

## Example Usage

### Before (Standalone Script):
```python
import json
import sys

# Global configuration
DEFAULT_CONFIG = {"timeout": 30, "retries": 3}

# Read command line arguments
if len(sys.argv) > 1:
    input_file = sys.argv[1]
else:
    input_file = "data.json"

# Process the data
with open(input_file, 'r') as f:
    data = json.load(f)

results = []
for item in data:
    if item.get('active', False):
        processed = {
            'id': item['id'],
            'name': item['name'].upper(),
            'score': item.get('score', 0) * 2
        }
        results.append(processed)

print(f"Processed {len(results)} items")
```

### After (CSnakes-Compatible):
```python
from typing import Optional

def process_data_file(
    input_file: str = "data.json",
    timeout: int = 30,
    retries: int = 3,
    score_multiplier: float = 2.0
) -> tuple[list[dict[str, str | int]], int]:
    """
    Process a JSON data file and return active items with transformed data.
    
    Args:
        input_file: Path to the JSON file to process
        timeout: Connection timeout in seconds
        retries: Number of retry attempts
        score_multiplier: Multiplier to apply to scores
        
    Returns:
        Tuple of (processed_items, count) where processed_items is a list
        of dictionaries containing id, name, and score, and count is the
        number of items processed.
    """
    import json
    
    try:
        with open(input_file, 'r') as f:
            data = json.load(f)
    except FileNotFoundError:
        return [], 0
    
    results = []
    for item in data:
        if item.get('active', False):
            processed = {
                'id': str(item['id']),  # Ensure string type
                'name': item['name'].upper(),
                'score': int(item.get('score', 0) * score_multiplier)  # Ensure int type
            }
            results.append(processed)
    
    return results, len(results)
```

**Generated C# signature:**
```csharp
public (IReadOnlyList<IReadOnlyDictionary<string, object>>, long) ProcessDataFile(
    string inputFile = "data.json", 
    long timeout = 30, 
    long retries = 3, 
    double scoreMultiplier = 2.0);
```

## Additional Tips

When using this prompt:

1. **Replace `[PASTE YOUR SCRIPT HERE]`** with your actual Python code
2. **Review the suggestions** - AI assistants might suggest breaking complex scripts into multiple functions
3. **Test the generated functions** in isolation before integrating with CSnakes
4. **Consider data flow** - make sure all necessary data is passed as parameters rather than using global variables

For more information about supported types and CSnakes features, see the [Reference Documentation](reference.md).

## Setup CSnakes Python Environment in Host Builder

**Problem**: You have an existing C# application using Microsoft.Extensions.Hosting and need to integrate CSnakes with proper Python environment configuration.

**Copy this prompt into your AI assistant:**

```
Add CSnakes Python environment configuration to my existing Microsoft.Extensions.Hosting application.

Requirements:
1. Configure CSnakes services with the Host Builder pattern
2. Set up Python locators with fallback chain for cross-platform compatibility
3. Configure virtual environment support if Python dependencies are needed
4. Include proper error handling for missing Python installations
5. Add IPythonEnvironment service resolution
6. Show how to call Python functions from the configured environment

Python locator options (choose based on deployment scenario):
- FromRedistributable() - Downloads Python automatically (recommended for most cases)
- FromVirtualEnvironment(path) - Suggested if using additional packages from Pip installer or UV installer
- FromConda(condaPath) - If using Conda environments

Additional configuration options:
- WithHome(path) - Path to your Python modules directory (required)
- WithVirtualEnvironment(path) - Path to virtual environment
- WithPipInstaller() - Auto-install packages from requirements.txt using the pip installer
- WithCondaEnvironment(name) - Use specific Conda environment

Here's my existing Host Builder code:
[PASTE YOUR HOST BUILDER CODE HERE]
```

## Example Usage

### Before (Basic Host Builder):
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Add other services
builder.Services.AddHttpClient();
builder.Services.AddScoped<MyService>();

var app = builder.Build();

// Run application
await app.RunAsync();
```

### After (With CSnakes Integration):
```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Services.AddLogging(configure => configure.AddConsole());

// Add other services
builder.Services.AddHttpClient();
builder.Services.AddScoped<MyService>();

// Configure CSnakes Python environment
var pythonHome = Path.Join(Environment.CurrentDirectory, "python_modules");
builder.Services
    .WithPython()
    .WithHome(pythonHome)
    .FromRedistributable()
    // Optional: Virtual environment configuration
    .WithVirtualEnvironment(Path.Join(pythonHome, ".venv"))
    .WithPipInstaller();  // Auto-install from requirements.txt

var app = builder.Build();

// Example: Using the Python environment
var pythonEnv = app.Services.GetRequiredService<IPythonEnvironment>();

try 
{
    // Call your Python functions here
    var myModule = pythonEnv.MyPythonModule();
    var result = myModule.MyFunction("Hello from C#!");
    Console.WriteLine($"Python returned: {result}");
}
catch (PythonInvocationException ex)
{
    Console.WriteLine($"Python error: {ex.Message}");
}

await app.RunAsync();
```

### Web Application Example:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CSnakes
var pythonHome = Path.Join(builder.Environment.ContentRootPath, "python");
builder.Services
    .WithPython()
    .WithHome(pythonHome)
    .FromRedistributable()
    .WithVirtualEnvironment(Path.Join(pythonHome, ".venv"))
    .WithPipInstaller();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();
app.MapControllers();

// Example API endpoint using Python
app.MapGet("/python-greeting/{name}", (string name, IPythonEnvironment env) =>
{
    try
    {
        var module = env.Greeting();
        return Results.Ok(new { message = module.SayHello(name) });
    }
    catch (PythonInvocationException ex)
    {
        return Results.Problem($"Python error: {ex.Message}");
    }
});

app.Run();
```

## Additional Tips

When using this prompt:

1. **Replace `[PASTE YOUR HOST BUILDER CODE HERE]`** with your actual Host Builder setup
2. **Choose appropriate locators** - FromRedistributable() is recommended for most scenarios
3. **Set correct paths** - Ensure WithHome() points to your Python modules directory
4. **Handle errors gracefully** - Always wrap Python calls in try-catch blocks
5. **Test Python isolation** - Verify your virtual environment setup if using external packages

For more information about supported types and CSnakes features, see the [Reference Documentation](reference.md).
