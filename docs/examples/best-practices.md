# Best Practices

This guide outlines best practices for developing, deploying, and maintaining applications with CSnakes.

## Project Organization

### Directory Structure

Organize your project with a clear separation between C# and Python code:

```
MyProject/
├── src/
│   ├── MyProject.csproj
│   ├── Program.cs
│   ├── Controllers/
│   └── Services/
├── python_modules/
│   ├── __init__.py
│   ├── data/
│   │   ├── __init__.py
│   │   ├── processors.py
│   │   └── validators.py
│   ├── ml/
│   │   ├── __init__.py
│   │   ├── models.py
│   │   └── training.py
│   └── utils/
│       ├── __init__.py
│       ├── helpers.py
│       └── constants.py
├── requirements.txt
├── .gitignore
└── README.md
```

### Project Configuration

**Always use explicit file inclusion in your `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="0.16.0" />
  </ItemGroup>

  <!-- Include all Python files -->
  <ItemGroup>
    <AdditionalFiles Include="python_modules/**/*.py">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>

  <!-- Include requirements.txt -->
  <ItemGroup>
    <Content Include="requirements.txt">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

## Python Code Best Practices

### Type Annotations

**Always use comprehensive type annotations:**

```python
# Good ✅
def process_user_data(
    user_id: int, 
    email: str, 
    preferences: dict[str, bool]
) -> tuple[bool, str, dict[str, any]]:
    """Process user data and return success status, message, and processed data."""
    try:
        # Processing logic here
        processed_data = {"user_id": user_id, "email": email.lower()}
        return True, "Success", processed_data
    except Exception as e:
        return False, str(e), {}

# Bad ❌
def process_user_data(user_id, email, preferences):
    # No type hints make this unusable from C#
    return True, "Success", {}
```

### Error Handling

**Use explicit error handling and meaningful exceptions:**

```python
# Good ✅
def validate_email(email: str) -> tuple[bool, str]:
    """Validate email format and return status with message."""
    if not email:
        return False, "Email cannot be empty"
    
    if "@" not in email:
        return False, "Email must contain @ symbol"
    
    if "." not in email.split("@")[1]:
        return False, "Email domain must contain a dot"
    
    return True, "Valid email"

def risky_operation(value: int) -> int:
    """Perform operation that might fail."""
    if value < 0:
        raise ValueError(f"Value must be non-negative, got {value}")
    
    if value > 1000:
        raise ValueError(f"Value too large, maximum is 1000, got {value}")
    
    return value * 2

# Bad ❌
def validate_email(email):
    # Silent failures are hard to debug
    return "@" in email and "." in email

def risky_operation(value):
    # Generic exceptions without context
    if value < 0:
        raise Exception("Bad value")
    return value * 2
```

### Documentation

**Document your Python functions thoroughly:**

```python
def calculate_discount(
    price: float, 
    customer_tier: str, 
    item_category: str
) -> tuple[float, float, str]:
    """
    Calculate discount for a customer purchase.
    
    Args:
        price: Original item price in USD
        customer_tier: Customer tier ('bronze', 'silver', 'gold', 'platinum')
        item_category: Item category ('electronics', 'clothing', 'books', etc.)
    
    Returns:
        Tuple of (discounted_price, discount_amount, discount_reason)
    
    Raises:
        ValueError: If price is negative or customer_tier is invalid
    
    Examples:
        >>> calculate_discount(100.0, 'gold', 'electronics')
        (85.0, 15.0, 'Gold tier 15% discount')
    """
    if price < 0:
        raise ValueError("Price cannot be negative")
    
    valid_tiers = ['bronze', 'silver', 'gold', 'platinum']
    if customer_tier not in valid_tiers:
        raise ValueError(f"Invalid customer tier: {customer_tier}")
    
    # Discount logic here...
    return discounted_price, discount_amount, reason
```

### Performance Considerations

**Optimize for repeated calls:**

```python
# Good ✅ - Cache expensive computations
import functools

@functools.lru_cache(maxsize=128)
def expensive_calculation(input_value: int) -> float:
    """Expensive calculation with caching."""
    # Complex computation here
    result = sum(i ** 2 for i in range(input_value))
    return float(result)

# Good ✅ - Use generators for large datasets
def process_large_dataset(data: list[dict[str, any]]) -> list[dict[str, any]]:
    """Process large dataset efficiently."""
    def process_item(item):
        # Processing logic
        return {"processed": True, **item}
    
    # Process in chunks to avoid memory issues
    chunk_size = 1000
    results = []
    
    for i in range(0, len(data), chunk_size):
        chunk = data[i:i + chunk_size]
        processed_chunk = [process_item(item) for item in chunk]
        results.extend(processed_chunk)
    
    return results

# Bad ❌ - No caching for expensive operations
def expensive_calculation(input_value: int) -> float:
    # This will be slow on repeated calls
    return float(sum(i ** 2 for i in range(input_value)))
```

## C# Code Best Practices

### Dependency Injection

**Use dependency injection for Python environment:**

```csharp
// Good ✅
public class DataProcessingService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<DataProcessingService> _logger;
    
    public DataProcessingService(
        IPythonEnvironment python, 
        ILogger<DataProcessingService> logger)
    {
        _python = python;
        _logger = logger;
    }
    
    public async Task<ProcessingResult> ProcessDataAsync(InputData data)
    {
        try
        {
            var processor = _python.DataProcessors();
            var result = await Task.Run(() => processor.ProcessData(data.Items));
            
            _logger.LogInformation("Successfully processed {Count} items", data.Items.Count);
            return new ProcessingResult { Success = true, Data = result };
        }
        catch (PythonInvocationException ex)
        {
            _logger.LogError(ex, "Python processing failed for {Count} items", data.Items.Count);
            return new ProcessingResult { Success = false, Error = ex.Message };
        }
    }
}

// Bad ❌
public class DataProcessingService
{
    public ProcessingResult ProcessData(InputData data)
    {
        // Creating environment each time is expensive
        var builder = Host.CreateApplicationBuilder();
        builder.Services.WithPython().WithHome(".").FromRedistributable();
        var app = builder.Build();
        var env = app.Services.GetRequiredService<IPythonEnvironment>();
        
        // No error handling
        var processor = env.DataProcessors();
        var result = processor.ProcessData(data.Items);
        
        return new ProcessingResult { Success = true, Data = result };
    }
}
```

### Error Handling

**Implement comprehensive error handling:**

```csharp
// Good ✅
public class PythonService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<PythonService> _logger;
    
    public async Task<Result<T>> ExecutePythonFunctionAsync<T>(
        Func<IPythonEnvironment, T> pythonCall,
        string operationName)
    {
        try
        {
            var result = await Task.Run(() => pythonCall(_python));
            _logger.LogDebug("Python operation {Operation} completed successfully", operationName);
            return Result<T>.Success(result);
        }
        catch (PythonInvocationException ex)
        {
            _logger.LogError(ex, "Python operation {Operation} failed: {Error}", 
                operationName, ex.Message);
            return Result<T>.Failure($"Python error in {operationName}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Python operation {Operation}", operationName);
            return Result<T>.Failure($"Unexpected error in {operationName}: {ex.Message}");
        }
    }
}

// Usage
var result = await pythonService.ExecutePythonFunctionAsync(
    env => env.DataProcessor().ProcessData(data),
    "data processing"
);

if (result.IsSuccess)
{
    return Ok(result.Value);
}
else
{
    return BadRequest(result.Error);
}

// Bad ❌
public T ExecutePythonFunction<T>(Func<IPythonEnvironment, T> pythonCall)
{
    // No error handling - exceptions will bubble up
    return pythonCall(_python);
}
```

### Configuration

**Use strongly-typed configuration:**

```csharp
// Good ✅
public class PythonConfiguration
{
    public string PythonHome { get; set; } = string.Empty;
    public string VirtualEnvironmentPath { get; set; } = string.Empty;
    public bool UsePipInstaller { get; set; } = true;
    public string PythonVersion { get; set; } = "3.12";
    public bool EnableDebugging { get; set; } = false;
}

// In Program.cs
builder.Services.Configure<PythonConfiguration>(
    builder.Configuration.GetSection("Python"));

builder.Services.AddSingleton<IPythonEnvironment>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<PythonConfiguration>>().Value;
    
    var pythonBuilder = serviceProvider
        .WithPython()
        .WithHome(config.PythonHome);
    
    if (!string.IsNullOrEmpty(config.VirtualEnvironmentPath))
    {
        pythonBuilder.WithVirtualEnvironment(config.VirtualEnvironmentPath);
    }
    
    if (config.UsePipInstaller)
    {
        pythonBuilder.WithPipInstaller();
    }
    
    return pythonBuilder.FromRedistributable(config.PythonVersion, debug: config.EnableDebugging);
});

// Bad ❌
// Hardcoded configuration scattered throughout code
builder.Services
    .WithPython()
    .WithHome("./python")
    .WithVirtualEnvironment("./.venv")
    .FromRedistributable("3.12");
```

## Environment Management

### Virtual Environments

**Always use virtual environments for production:**

```csharp
// Good ✅
builder.Services
    .WithPython()
    .WithHome(pythonModulesPath)
    .WithVirtualEnvironment(venvPath)
    .WithPipInstaller() // Automatically install requirements.txt
    .FromRedistributable();

// Requirements.txt management
# requirements.txt
numpy==1.24.3
pandas==2.0.3
scikit-learn==1.3.0
# Pin versions for reproducible builds
```

### Environment Variables

**Use environment variables for configuration:**

```csharp
// Good ✅
var pythonHome = Environment.GetEnvironmentVariable("PYTHON_HOME") 
    ?? Path.Combine(Environment.CurrentDirectory, "python");

var useVenv = Environment.GetEnvironmentVariable("USE_PYTHON_VENV")?.ToLower() == "true";

builder.Services
    .WithPython()
    .WithHome(pythonHome);

if (useVenv)
{
    var venvPath = Environment.GetEnvironmentVariable("PYTHON_VENV_PATH") 
        ?? Path.Combine(pythonHome, ".venv");
    builder.Services.WithVirtualEnvironment(venvPath);
}
```

## Testing Best Practices

### Unit Testing

**Test both C# and Python components:**

```csharp
// Good ✅
[TestClass]
public class DataProcessorTests
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
    public void ProcessData_ValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var processor = _python.DataProcessor();
        var input = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = processor.ProcessNumbers(input);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(input.Length, result.Count);
        CollectionAssert.AreEqual(new[] { 2L, 4L, 6L, 8L, 10L }, result.ToArray());
    }
    
    [TestMethod]
    public void ProcessData_InvalidInput_ThrowsException()
    {
        // Arrange
        var processor = _python.DataProcessor();
        
        // Act & Assert
        Assert.ThrowsException<PythonInvocationException>(
            () => processor.ProcessNumbers(null));
    }
}
```

### Integration Testing

**Test the full C#-Python integration:**

```csharp
[TestClass]
public class IntegrationTests
{
    [TestMethod]
    public async Task EndToEndDataProcessing_ValidWorkflow_Succeeds()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers();
        builder.Services
            .WithPython()
            .WithHome("./python_modules")
            .FromRedistributable();
        
        var app = builder.Build();
        app.MapControllers();
        
        // Act
        using var client = new TestClient(app);
        var response = await client.PostAsJsonAsync("/api/data/process", testData);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProcessingResult>();
        Assert.IsTrue(result.Success);
    }
}
```

## Security Best Practices

### Input Validation

**Always validate inputs before passing to Python:**

```csharp
// Good ✅
public class SecureDataProcessor
{
    private readonly IPythonEnvironment _python;
    
    public ProcessingResult ProcessUserData(UserDataRequest request)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.UserId))
            return ProcessingResult.Failure("User ID is required");
        
        if (request.Data?.Count > 10000)
            return ProcessingResult.Failure("Data size exceeds maximum limit");
        
        // Sanitize string inputs
        var sanitizedUserId = request.UserId.Replace(";", "").Replace("'", "");
        
        try
        {
            var processor = _python.SecureProcessor();
            var result = processor.ProcessUserData(sanitizedUserId, request.Data);
            return ProcessingResult.Success(result);
        }
        catch (PythonInvocationException ex)
        {
            // Don't expose Python internals to users
            _logger.LogError(ex, "Processing failed for user {UserId}", sanitizedUserId);
            return ProcessingResult.Failure("Processing failed");
        }
    }
}
```

### Resource Management

**Implement proper resource management:**

```csharp
// Good ✅
public class ResourceAwareService : IDisposable
{
    private readonly IPythonEnvironment _python;
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;
    
    public ResourceAwareService(IPythonEnvironment python)
    {
        _python = python;
        _semaphore = new SemaphoreSlim(10); // Limit concurrent operations
    }
    
    public async Task<T> ExecuteWithLimitAsync<T>(Func<T> operation)
    {
        await _semaphore.WaitAsync();
        try
        {
            return operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore?.Dispose();
            _disposed = true;
        }
    }
}
```

## Performance Best Practices

### Optimize for Your Use Case

**Choose the right approach based on your needs:**

```csharp
// For frequent, small operations - keep environment warm
public class HighFrequencyService
{
    private readonly IPythonEnvironment _python;
    
    public HighFrequencyService(IPythonEnvironment python)
    {
        _python = python;
        // Pre-warm the environment
        _ = _python.QuickOperations();
    }
}

// For infrequent, large operations - use background processing
public class BatchProcessingService
{
    public async Task ProcessLargeBatchAsync(IEnumerable<DataItem> items)
    {
        await Task.Run(() =>
        {
            var processor = _python.BatchProcessor();
            foreach (var batch in items.Chunk(1000))
            {
                processor.ProcessBatch(batch);
            }
        });
    }
}
```

## Deployment Best Practices

### Container Deployment

**Use multi-stage Docker builds:**

```dockerfile
# Good ✅
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY *.csproj ./
RUN dotnet restore

# Copy source code
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install Python
RUN apt-get update && apt-get install -y python3 python3-pip
COPY requirements.txt ./
RUN pip3 install -r requirements.txt

# Copy application
COPY --from=build /app/out ./
COPY python_modules ./python_modules/

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Environment Configuration

**Use configuration providers:**

```json
// appsettings.Production.json
{
  "Python": {
    "PythonHome": "/app/python_modules",
    "VirtualEnvironmentPath": "/app/.venv",
    "UsePipInstaller": false,
    "PythonVersion": "3.12",
    "EnableDebugging": false
  },
  "Logging": {
    "LogLevel": {
      "CSnakes": "Warning"
    }
  }
}
```

## Next Steps

- [Review advanced topics](../advanced/advanced-usage.md)
- [Understand performance optimization](../advanced/performance.md)
- [Learn about troubleshooting](../advanced/troubleshooting.md)
