# Troubleshooting

This guide helps you diagnose and resolve common issues when working with CSnakes.

## Common Issues and Solutions

### Python Environment Issues

#### Issue: "Python runtime not found"

**Symptoms:**

- Error message: "Could not locate Python runtime"
- Application fails to start
- PythonEnvironment initialization throws exception

**Solutions:**

1. **Use Redistributable Locator (Recommended)**
   ```csharp
   builder.Services
       .WithPython()
       .WithHome(pythonHome)
       .FromRedistributable(); // Automatically downloads Python
   ```

2. **Set Environment Variable**
   ```bash
   # Windows
   set PYTHONHOME=C:\Python312

   # Linux/macOS
   export PYTHONHOME=/usr/local/python3.12
   ```

3. **Use Explicit Path**
   ```csharp
   builder.Services
       .WithPython()
       .WithHome(pythonHome)
       .FromFolder(@"C:\Python312", "3.12");
   ```

#### Issue: "Module not found" errors

**Symptoms:**

- `ModuleNotFoundError` in Python code
- ImportError for installed packages
- Functions work in Python CLI but not in CSnakes

**Solutions:**

1. **Use Virtual Environment**
   ```csharp
   builder.Services
       .WithPython()
       .WithHome(pythonHome)
       .WithVirtualEnvironment(Path.Combine(pythonHome, ".venv"))
       .WithPipInstaller() // Installs requirements.txt automatically
       .FromRedistributable();
   ```

2. **Verify Requirements.txt**

   ```text
   # requirements.txt
   numpy==1.24.3
   pandas==2.0.3
   scikit-learn==1.3.0
   ```

3. **Manual Package Installation**

   ```bash
   # In your virtual environment
   pip install -r requirements.txt
   ```

4. **Check Python Path**

   ```python
   # Add to your Python file for debugging
   import sys
   print("Python path:", sys.path)
   print("Python executable:", sys.executable)
   ```

### Build and Source Generation Issues

#### Issue: "Generated code not found"

**Symptoms:**

- IntelliSense doesn't show Python methods
- Compiler errors about missing methods
- `env.ModuleName()` not available

**Solutions:**

1. **Check File Configuration**

   ```xml
   <ItemGroup>
     <AdditionalFiles Include="python_modules/**/*.py" SourceItemType="Python">
       <CopyToOutputDirectory>Always</CopyToOutputDirectory>
     </AdditionalFiles>
   </ItemGroup>
   ```

2. **Verify Python File Syntax**

   ```python
   # Ensure proper type annotations
   def my_function(param: str) -> str:  # ✅ Good
       return f"Hello {param}"

   def bad_function(param):  # ❌ Bad - no type hints
       return f"Hello {param}"
   ```

3. **Clean and Rebuild**

   ```bash
   dotnet clean
   dotnet build
   ```

4. **Check Build Output**

   - Look for source generation errors in build output
   - Verify Python files are being copied to output directory

#### Issue: "Type conversion errors"

**Symptoms:**

- Runtime exceptions during type conversion
- Unexpected `null` values
- Type mismatch errors

**Solutions:**

1. **Use Supported Types**
   ```python
   # ✅ Supported types
   def good_function(
       text: str,
       number: int,
       items: list[str],
       mapping: dict[str, int]
   ) -> tuple[str, int]:
       return text, number

   # ❌ Unsupported complex types
   def bad_function(custom_object: MyCustomClass) -> MyCustomClass:
       return custom_object
   ```

2. **Handle Optional Types Properly**

   ```python
   def safe_function(value: str | None = None) -> str:
       if value is None:
           return "default"
       return value
   ```

   ```csharp
   // C# usage
   string result1 = module.SafeFunction();           // Uses default
   string result2 = module.SafeFunction("custom");   // Uses provided value
   string? result3 = module.SafeFunction(null);      // Explicitly pass null
   ```

### Runtime Errors

#### Issue: "PythonInvocationException"

**Symptoms:**

- Python exceptions bubble up to C#
- Stack traces point to Python code
- Application crashes on Python errors

**Solutions:**

1. **Proper Error Handling**
   ```csharp
   try
   {
       var result = module.RiskyFunction(input);
       return result;
   }
   catch (PythonInvocationException ex)
   {
       _logger.LogError(ex, "Python function failed with input: {Input}", input);

       // Check specific Python exception type
       if (ex.PythonExceptionType == "ValueError")
       {
           return HandleValueError(ex);
       }

       throw; // Re-throw if can't handle
   }
   ```

2. **Defensive Python Code**
   ```python
   def robust_function(value: int) -> tuple[bool, int, str]:
       """Return (success, result, error_message)"""
       try:
           if value < 0:
               return False, 0, "Value must be non-negative"

           result = expensive_operation(value)
           return True, result, ""

       except Exception as e:
           return False, 0, str(e)
   ```

3. **Input Validation**
   ```csharp
   public ProcessingResult ProcessData(InputData data)
   {
       // Validate before calling Python
       if (data?.Items == null || !data.Items.Any())
       {
           return ProcessingResult.Failure("No data provided");
       }

       if (data.Items.Count > MaxItemCount)
       {
           return ProcessingResult.Failure($"Too many items: {data.Items.Count}");
       }

       try
       {
           var result = module.ProcessItems(data.Items);
           return ProcessingResult.Success(result);
       }
       catch (PythonInvocationException ex)
       {
           return ProcessingResult.Failure($"Processing failed: {ex.Message}");
       }
   }
   ```

### Performance Issues

#### Issue: "Slow startup times"

**Symptoms:**

- Application takes long time to start
- First Python call is very slow
- High memory usage during startup

**Solutions:**

1. **Use Environment Warming**
   ```csharp
   public class StartupService : IHostedService
   {
       private readonly IPythonEnvironment _python;

       public StartupService(IPythonEnvironment python)
       {
           _python = python;
       }

       public async Task StartAsync(CancellationToken cancellationToken)
       {
           // Warm up Python environment
           await Task.Run(() =>
           {
               var module = _python.MyModule();
               // Make a simple call to initialize everything
               module.WarmupFunction();
           }, cancellationToken);
       }

       public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
   }
   ```

2. **Optimize Python Imports**

   ```python
   # ✅ Import only what you need
   from sklearn.cluster import KMeans  # Specific import
   import numpy as np

   # ❌ Avoid importing everything
   from sklearn import *  # Imports everything, slows startup
   import pandas  # Large library, import only if needed
   ```

3. **Use Lazy Loading**

   ```python
   def process_with_heavy_imports(data: list[int]) -> list[int]:
       """Import heavy libraries only when needed."""
       import tensorflow as tf  # Import inside function
       import torch

       # Processing logic
       return processed_data
   ```

#### Issue: "High memory usage"

**Symptoms:**

- Memory usage grows over time
- OutOfMemoryException
- Application becomes unresponsive

**Solutions:**

1. **Implement Resource Management**

   ```csharp
   public class ResourceManagedService
   {
       private readonly IPythonEnvironment _python;
       private readonly SemaphoreSlim _semaphore;

       public ResourceManagedService(IPythonEnvironment python)
       {
           _python = python;
           _semaphore = new SemaphoreSlim(Environment.ProcessorCount);
       }

       public async Task<T> ExecuteAsync<T>(Func<T> operation)
       {
           await _semaphore.WaitAsync();
           try
           {
               return operation();
           }
           finally
           {
               _semaphore.Release();

               // Force garbage collection if needed
               if (GC.GetTotalMemory(false) > 500_000_000) // 500MB threshold
               {
                   GC.Collect();
                   GC.WaitForPendingFinalizers();
               }
           }
       }
   }
   ```

2. **Optimize Python Memory Usage**
   ```python
   def memory_efficient_processing(large_data: list[dict]) -> list[dict]:
       """Process data in chunks to manage memory."""
       chunk_size = 1000
       results = []

       for i in range(0, len(large_data), chunk_size):
           chunk = large_data[i:i + chunk_size]
           processed_chunk = process_chunk(chunk)
           results.extend(processed_chunk)

           # Clear intermediate variables
           del chunk, processed_chunk

       return results
   ```

### AOT (Ahead-of-Time) Compilation Issues

#### Issue: "AOT compilation fails"

**Symptoms:**

- Build errors during AOT compilation
- Runtime errors in AOT-compiled application
- Missing dependencies in AOT build

**Solutions:**

1. **Use Source Generation Only**
   ```csharp
   // ✅ Good for AOT
   var module = env.MyModule();
   var result = module.MyFunction(input);

   // ❌ Bad for AOT - runtime binding not supported
   // env.GetModule("my_module").InvokeFunction("my_function", input);
   ```

2. **Configure AOT Properly**

   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <PublishAot>true</PublishAot>
       <SelfContained>true</SelfContained>
       <RuntimeIdentifier>win-x64</RuntimeIdentifier>
     </PropertyGroup>
   </Project>
   ```

3. **Trim-Safe Code**

   ```csharp
   // Use attributes to preserve code from trimming
   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
   public class MyService
   {
       // Implementation
   }
   ```

## Debugging Techniques

### Enable Detailed Logging

```csharp
// Configure logging in Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add specific logging for CSnakes
builder.Logging.AddFilter("CSnakes", LogLevel.Debug);
```

### Python Debugging

```python
# Add debugging output to Python functions
import logging
import sys

logging.basicConfig(level=logging.DEBUG)
logger = logging.getLogger(__name__)

def debug_function(input_data: list[int]) -> list[int]:
    logger.debug(f"Input received: {input_data}")
    logger.debug(f"Python version: {sys.version}")
    logger.debug(f"Python path: {sys.path}")

    try:
        result = process_data(input_data)
        logger.debug(f"Processing result: {result}")
        return result
    except Exception as e:
        logger.error(f"Processing failed: {e}")
        raise
```

### Environment Diagnostics

```csharp
public class DiagnosticsService
{
    private readonly IPythonEnvironment _python;

    public DiagnosticsInfo GetDiagnostics()
    {
        try
        {
            var diagnostics = _python.Diagnostics();
            return new DiagnosticsInfo
            {
                PythonVersion = diagnostics.GetPythonVersion(),
                PythonPath = diagnostics.GetPythonPath(),
                InstalledPackages = diagnostics.GetInstalledPackages(),
                EnvironmentVariables = diagnostics.GetEnvironmentVariables()
            };
        }
        catch (Exception ex)
        {
            return new DiagnosticsInfo
            {
                Error = ex.Message,
                StackTrace = ex.StackTrace
            };
        }
    }
}
```
### Logging Configuration Example
To see detailed logs, for pip installation, configure logging as follows in appsettings.json:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```
You may also need to set it in your .csproj:
```xml
  <ItemGroup>
    <Content Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```
You can set also have configuration for different deployment environments like Development, Staging, Production, etc. Additionally, you can set logging levels for specific namespaces, e.g., "Microsoft.AspNetCore": "Warning".
For further example, see [Aspire Sample Project](https://github.com/tonybaloney/CSnakes/tree/main/samples/Aspire).
## Getting Help

### Check Logs First

Always check application logs for detailed error information:

```csharp
// Enable comprehensive logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

### Create Minimal Reproduction

When seeking help, create a minimal example:

```python
# minimal_example.py
def simple_function(value: int) -> int:
    return value * 2
```

```csharp
// Minimal C# program
var builder = Host.CreateApplicationBuilder();
builder.Services
    .WithPython()
    .WithHome(".")
    .FromRedistributable();

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

var module = env.MinimalExample();
var result = module.SimpleFunction(5);
Console.WriteLine($"Result: {result}");
```

### Useful Resources

- [CSnakes GitHub Issues](https://github.com/tonybaloney/CSnakes/issues)
- [Sample Projects](../examples/sample-projects.md)
- [Performance Guide](performance.md)

## Environment-Specific Issues

### Windows Issues

**Issue: "Access denied" errors**
- Solution: Run as administrator or check file permissions
- Ensure antivirus isn't blocking Python execution

**Issue: "Long path names"**
- Solution: Enable long path support in Windows
- Use shorter directory names

### Linux/macOS Issues

**Issue: "Permission denied"**
- Solution: Check execute permissions on Python binary
- Ensure user has access to Python installation directory

**Issue: "Shared library errors"**
- Solution: Install required system dependencies
- Check `LD_LIBRARY_PATH` environment variable

### Docker Issues

**Issue: "Python not found in container"**

- Solution: Install Python in Dockerfile
- Use base images with Python pre-installed
- Use [CSnakes.Stage](../user-guide/deployment.md)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y python3 python3-pip
COPY requirements.txt ./
RUN pip3 install -r requirements.txt
```

## Next Steps

- [Learn about performance optimization](performance.md)
- [Explore advanced usage patterns](advanced-usage.md)
