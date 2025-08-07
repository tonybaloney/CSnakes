# Hot Reload & Development

This guide covers development workflows, hot reload capabilities, and debugging techniques for CSnakes applications.

## Hot Reload Support

CSnakes supports hot reload of Python code in supported IDEs, allowing you to modify Python functions and see changes immediately without restarting your application.

### Enabling Hot Reload

Hot reload is automatically available when using CSnakes with:
- Visual Studio 2022 (with Hot Reload enabled)
- Visual Studio Code (with C# Dev Kit)
- JetBrains Rider

### How Hot Reload Works

1. **Modify Python File**: Change your Python function
2. **Automatic Detection**: CSnakes detects file changes
3. **Module Reload**: Python module is reloaded with new code
4. **Immediate Effect**: Next function call uses updated code

### Example Hot Reload Workflow

**Initial Python Code**:
```python
# calculator.py
def add(a: int, b: int) -> int:
    return a + b

def multiply(a: int, b: int) -> int:
    return a * b
```

**C# Code**:
```csharp
var calculator = env.Calculator();

while (true)
{
    Console.WriteLine("Choose operation: 1=Add, 2=Multiply, 3=Exit");
    var choice = Console.ReadLine();
    
    switch (choice)
    {
        case "1":
            var sum = calculator.Add(5, 3);
            Console.WriteLine($"5 + 3 = {sum}");
            break;
        case "2":
            var product = calculator.Multiply(5, 3);
            Console.WriteLine($"5 * 3 = {product}");
            break;
        case "3":
            return;
    }
}
```

**Modified Python Code** (while application is running):
```python
# calculator.py
def add(a: int, b: int) -> int:
    print(f"Adding {a} and {b}")  # Added logging
    return a + b

def multiply(a: int, b: int) -> int:
    print(f"Multiplying {a} and {b}")  # Added logging
    return a * b * 2  # Changed logic - double the result!
```

The next time you call `multiply`, it will use the new logic without restarting the application.

### Hot Reload Configuration

**Enable Hot Reload in Visual Studio**:
1. Go to Debug → Windows → Hot Reload
2. Ensure "Enable Hot Reload" is checked
3. Set "Apply code changes" to automatic

**Enable Hot Reload in VS Code**:
```json
// .vscode/launch.json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (console)",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/bin/Debug/net8.0/MyApp.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false,
            "hotReloadEnabled": true
        }
    ]
}
```

### Hot Reload Limitations

**What Works**:
- Function body changes
- Adding new functions
- Modifying return values
- Changing logic inside functions

**What Doesn't Work**:
- Changing function signatures (parameters or return types)
- Adding/removing function parameters
- Changing type annotations
- Adding new dependencies/imports

## Development Workflows

### Interactive Development

**Setup for Interactive Development**:
```csharp
public class InteractiveDevelopment
{
    private readonly IPythonEnvironment _python;
    
    public InteractiveDevelopment()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services
            .WithPython()
            .WithHome("./python_modules")
            .FromRedistributable();
        
        var app = builder.Build();
        _python = app.Services.GetRequiredService<IPythonEnvironment>();
    }
    
    public void StartInteractiveSession()
    {
        var processor = _python.DataProcessor();
        
        while (true)
        {
            Console.Write("Enter command (process/test/exit): ");
            var command = Console.ReadLine();
            
            switch (command?.ToLower())
            {
                case "process":
                    TestProcessing(processor);
                    break;
                case "test":
                    RunTests(processor);
                    break;
                case "exit":
                    return;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }
    }
    
    private void TestProcessing(dynamic processor)
    {
        try
        {
            var testData = new[] { 1, 2, 3, 4, 5 };
            var result = processor.ProcessNumbers(testData);
            Console.WriteLine($"Result: [{string.Join(", ", result)}]");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

### File Watching for Development

**Implement Custom File Watching**:
```csharp
public class PythonFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly IPythonEnvironment _python;
    private readonly ILogger<PythonFileWatcher> _logger;
    
    public PythonFileWatcher(IPythonEnvironment python, ILogger<PythonFileWatcher> logger)
    {
        _python = python;
        _logger = logger;
        
        _watcher = new FileSystemWatcher("./python_modules", "*.py")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        
        _watcher.Changed += OnPythonFileChanged;
    }
    
    private void OnPythonFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Python file changed: {FileName}", e.Name);
        
        try
        {
            // Trigger reload by accessing the module
            var moduleName = Path.GetFileNameWithoutExtension(e.Name);
            var module = _python.GetType()
                .GetMethod(char.ToUpper(moduleName[0]) + moduleName[1..])
                ?.Invoke(_python, null);
            
            _logger.LogInformation("Module {ModuleName} reloaded successfully", moduleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload module after file change");
        }
    }
    
    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
```

## Debugging Python Code

### Adding Debug Output

**Python Side Debugging**:
```python
# debug_utils.py
import logging
import traceback
import sys
from typing import Any, Callable, TypeVar

# Configure logging
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

T = TypeVar('T')

def debug_function(func: Callable[..., T]) -> Callable[..., T]:
    """Decorator to add debugging to functions."""
    def wrapper(*args, **kwargs):
        logger.debug(f"Calling {func.__name__} with args={args}, kwargs={kwargs}")
        try:
            result = func(*args, **kwargs)
            logger.debug(f"{func.__name__} returned: {result}")
            return result
        except Exception as e:
            logger.error(f"{func.__name__} failed: {e}")
            logger.error(f"Traceback: {traceback.format_exc()}")
            raise
    return wrapper

@debug_function
def process_data(data: list[int]) -> list[int]:
    """Process data with debugging."""
    logger.debug(f"Input data type: {type(data)}, length: {len(data)}")
    
    if not data:
        raise ValueError("Data cannot be empty")
    
    result = [x * 2 for x in data if x > 0]
    logger.debug(f"Filtered {len(data) - len(result)} negative values")
    
    return result

def get_debug_info() -> dict[str, Any]:
    """Get Python environment debug information."""
    return {
        "python_version": sys.version,
        "python_executable": sys.executable,
        "python_path": sys.path,
        "modules": list(sys.modules.keys())
    }
```

### C# Side Debugging

**Comprehensive Error Handling**:
```csharp
public class DebugService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<DebugService> _logger;
    
    public DebugService(IPythonEnvironment python, ILogger<DebugService> logger)
    {
        _python = python;
        _logger = logger;
    }
    
    public async Task<T> ExecuteWithDebugging<T>(
        string operationName,
        Func<T> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Starting operation: {OperationName}", operationName);
            
            var result = await Task.Run(operation);
            
            stopwatch.Stop();
            _logger.LogDebug("Operation {OperationName} completed in {ElapsedMs}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (PythonInvocationException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, 
                "Python operation {OperationName} failed after {ElapsedMs}ms. " +
                "Python exception: {PythonType} - {PythonMessage}\n" +
                "Python stack trace:\n{PythonStackTrace}",
                operationName, 
                stopwatch.ElapsedMilliseconds,
                ex.PythonExceptionType,
                ex.Message,
                ex.PythonStackTrace);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, 
                "Operation {OperationName} failed after {ElapsedMs}ms with unexpected error",
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    public void PrintDebugInfo()
    {
        try
        {
            var debugUtils = _python.DebugUtils();
            var info = debugUtils.GetDebugInfo();
            
            _logger.LogInformation("Python Debug Information:");
            foreach (var kvp in info)
            {
                _logger.LogInformation("  {Key}: {Value}", kvp.Key, kvp.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Python debug information");
        }
    }
}
```

### Debugging Configuration

**Development Configuration**:
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "CSnakes": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "Python": {
    "EnableDebugging": true,
    "VerboseLogging": true,
    "ReloadOnChange": true
  }
}
```

**Debug-Enabled Python Environment**:
```csharp
public static class PythonEnvironmentExtensions
{
    public static IPythonEnvironmentBuilder WithDebugMode(
        this IPythonEnvironmentBuilder builder, 
        bool enableDebug = true)
    {
        if (enableDebug)
        {
            // Add debug-specific configuration
            Environment.SetEnvironmentVariable("PYTHONVERBOSE", "1");
            Environment.SetEnvironmentVariable("PYTHONDEBUG", "1");
        }
        
        return builder;
    }
}

// Usage
builder.Services
    .WithPython()
    .WithHome("./python_modules")
    .WithDebugMode(true)  // Enable debug mode
    .FromRedistributable();
```

## Testing During Development

### Unit Testing Python Functions

**Test Python Functions Directly**:
```python
# test_calculator.py
import unittest
from calculator import add, multiply

class TestCalculator(unittest.TestCase):
    def test_add(self):
        self.assertEqual(add(2, 3), 5)
        self.assertEqual(add(-1, 1), 0)
        self.assertEqual(add(0, 0), 0)
    
    def test_multiply(self):
        self.assertEqual(multiply(2, 3), 6)
        self.assertEqual(multiply(-1, 5), -5)
        self.assertEqual(multiply(0, 100), 0)

if __name__ == '__main__':
    unittest.main()
```

**Integration Testing with Hot Reload**:
```csharp
[TestClass]
public class HotReloadTests
{
    private IPythonEnvironment _python;
    
    [TestInitialize]
    public void Setup()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services
            .WithPython()
            .WithHome("./test_python_modules")
            .FromRedistributable();
        
        var app = builder.Build();
        _python = app.Services.GetRequiredService<IPythonEnvironment>();
    }
    
    [TestMethod]
    public void TestFunctionReload()
    {
        var calculator = _python.Calculator();
        
        // Initial test
        var result1 = calculator.Add(2, 3);
        Assert.AreEqual(5L, result1);
        
        // Simulate file change (in real scenario, modify the Python file)
        // The next call should pick up changes automatically
        var result2 = calculator.Add(2, 3);
        Assert.AreEqual(5L, result2); // Or new expected value after modification
    }
}
```

## Development Tools Integration

### Visual Studio Integration

**Task Configuration for Python Testing**:
```json
// .vscode/tasks.json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "test-python",
            "type": "shell",
            "command": "python",
            "args": ["-m", "pytest", "python_modules/tests/"],
            "group": "test",
            "presentation": {
                "echo": true,
                "reveal": "always",
                "focus": false,
                "panel": "shared"
            }
        },
        {
            "label": "lint-python",
            "type": "shell",
            "command": "python",
            "args": ["-m", "flake8", "python_modules/"],
            "group": "test"
        }
    ]
}
```

### Live Reloading Web Applications

**ASP.NET Core with Hot Reload**:
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        
        services
            .WithPython()
            .WithHome("./python_modules")
            .FromRedistributable();
        
        // Add file watching for development
        if (Environment.IsDevelopment())
        {
            services.AddSingleton<PythonFileWatcher>();
        }
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            
            // Start file watcher
            var fileWatcher = app.ApplicationServices.GetService<PythonFileWatcher>();
        }
        
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Performance Monitoring During Development

**Real-time Performance Metrics**:
```csharp
public class PerformanceMonitoringService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly ConcurrentDictionary<string, List<long>> _performanceData = new();
    
    public async Task<T> MonitoredExecute<T>(
        string operationName,
        Func<T> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await Task.Run(operation);
            
            stopwatch.Stop();
            RecordPerformance(operationName, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        finally
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
                RecordPerformance(operationName, stopwatch.ElapsedMilliseconds);
            }
        }
    }
    
    private void RecordPerformance(string operationName, long elapsedMs)
    {
        _performanceData.AddOrUpdate(
            operationName,
            new List<long> { elapsedMs },
            (key, list) =>
            {
                list.Add(elapsedMs);
                if (list.Count > 100) // Keep only last 100 measurements
                {
                    list.RemoveAt(0);
                }
                return list;
            });
        
        // Log if operation is slow
        if (elapsedMs > 1000) // 1 second threshold
        {
            _logger.LogWarning("Slow operation detected: {OperationName} took {ElapsedMs}ms", 
                operationName, elapsedMs);
        }
    }
    
    public Dictionary<string, object> GetPerformanceStats()
    {
        var stats = new Dictionary<string, object>();
        
        foreach (var kvp in _performanceData)
        {
            var times = kvp.Value;
            if (times.Any())
            {
                stats[kvp.Key] = new
                {
                    Average = times.Average(),
                    Min = times.Min(),
                    Max = times.Max(),
                    Count = times.Count,
                    Last = times.Last()
                };
            }
        }
        
        return stats;
    }
}
```

## Next Steps

- [Learn about performance optimization](performance.md)
- [Explore advanced usage patterns](advanced-usage.md)
- [Review troubleshooting guides](troubleshooting.md)
