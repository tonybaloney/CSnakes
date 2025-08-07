# Error Reference

This reference provides comprehensive information about error handling, exception types, and troubleshooting techniques in CSnakes.

## Exception Hierarchy

```
Exception
├── PythonRuntimeException           // Python runtime initialization errors
├── PythonInvocationException        // Python function execution errors
├── PythonStopIterationException     // Generator completion
├── InvalidCastException             // Type conversion errors
└── ArgumentException                // Invalid arguments to CSnakes APIs
```

## Core Exception Types

### PythonInvocationException

Thrown when Python code raises an exception during execution.

```csharp
public class PythonInvocationException : Exception
{
    // Python-specific information
    public string PythonExceptionType { get; }      // e.g., "ValueError", "TypeError"
    public string PythonStackTrace { get; }         // Python stack trace
    public Dictionary<string, PyObject> PythonLocals { get; }   // Local variables
    public Dictionary<string, PyObject> PythonGlobals { get; }  // Global variables
    
    // Standard exception properties
    public override string Message { get; }
    public Exception InnerException { get; }
}
```

**Common Python Exception Types:**
- `ValueError`: Invalid value passed to function
- `TypeError`: Wrong type passed to function
- `KeyError`: Dictionary key not found
- `IndexError`: List/array index out of range
- `AttributeError`: Object attribute not found
- `ImportError`: Module import failed
- `FileNotFoundError`: File operation failed
- `PermissionError`: Insufficient permissions

**Example Usage:**
```csharp
try
{
    var result = module.RiskyFunction("invalid_input");
}
catch (PythonInvocationException ex)
{
    Console.WriteLine($"Python Error Type: {ex.PythonExceptionType}");
    Console.WriteLine($"Error Message: {ex.Message}");
    Console.WriteLine($"Python Stack Trace:\n{ex.PythonStackTrace}");
    
    // Access Python variables at exception point
    if (ex.PythonLocals.ContainsKey("error_context"))
    {
        var context = ex.PythonLocals["error_context"];
        Console.WriteLine($"Error Context: {context}");
    }
}
```

### PythonRuntimeException

Thrown when there are issues with Python runtime initialization or configuration.

```csharp
public class PythonRuntimeException : Exception
{
    public PythonRuntimeException(string message);
    public PythonRuntimeException(string message, Exception innerException);
}
```

**Common Scenarios:**
- Python runtime not found
- Invalid Python version
- Virtual environment corruption
- Missing Python libraries
- Permission issues

**Example:**
```csharp
try
{
    var env = serviceProvider.GetRequiredService<IPythonEnvironment>();
}
catch (PythonRuntimeException ex)
{
    _logger.LogError(ex, "Failed to initialize Python runtime");
    
    // Provide user-friendly error message
    throw new ApplicationException(
        "Python runtime could not be initialized. Please check your Python installation.",
        ex);
}
```

### PythonStopIterationException

Thrown when a Python generator reaches the end of iteration.

```csharp
public class PythonStopIterationException : Exception
{
    public PyObject ReturnValue { get; }    // Generator return value
    
    public PythonStopIterationException(PyObject returnValue);
}
```

**Example:**
```python
def counting_generator(max_count: int) -> Generator[int, None, str]:
    for i in range(max_count):
        yield i
    return f"Counted to {max_count}"
```

```csharp
var generator = module.CountingGenerator(3);

try
{
    while (true)
    {
        var value = generator.Send(null);
        Console.WriteLine(value);
    }
}
catch (PythonStopIterationException ex)
{
    var returnValue = ex.ReturnValue.As<string>();
    Console.WriteLine($"Generator finished: {returnValue}");
}
```

## Type Conversion Errors

### InvalidCastException

Thrown when automatic type conversion fails.

```csharp
try
{
    // Python function returns dict, but we expect string
    string result = module.GetDictionary().As<string>();
}
catch (InvalidCastException ex)
{
    Console.WriteLine($"Type conversion failed: {ex.Message}");
    
    // Try alternative approach
    var obj = module.GetDictionary();
    if (obj.TryConvert<Dictionary<string, object>>(out var dict))
    {
        // Handle as dictionary
        ProcessDictionary(dict);
    }
}
```

### Safe Type Conversion

```csharp
public T SafeConvert<T>(PyObject pyObject, T defaultValue = default)
{
    try
    {
        return pyObject.As<T>();
    }
    catch (InvalidCastException)
    {
        _logger.LogWarning("Failed to convert PyObject to {Type}, using default", typeof(T));
        return defaultValue;
    }
}

// Usage
using var result = module.GetSomething();
var stringValue = SafeConvert<string>(result, "default");
var intValue = SafeConvert<long>(result, 0L);
```

## Error Handling Patterns

### Comprehensive Error Handling

```csharp
public class RobustPythonService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<RobustPythonService> _logger;
    
    public async Task<Result<T>> ExecuteSafelyAsync<T>(
        string operationName,
        Func<T> operation,
        int maxRetries = 3)
    {
        var attempt = 0;
        
        while (attempt <= maxRetries)
        {
            try
            {
                var result = await Task.Run(operation);
                return Result<T>.Success(result);
            }
            catch (PythonInvocationException ex)
            {
                _logger.LogError(ex, 
                    "Python operation {Operation} failed on attempt {Attempt}. " +
                    "Python exception: {PythonType} - {Message}",
                    operationName, attempt + 1, ex.PythonExceptionType, ex.Message);
                
                if (IsRetriableError(ex))
                {
                    attempt++;
                    if (attempt <= maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
                        continue;
                    }
                }
                
                return Result<T>.Failure($"Python operation failed: {ex.Message}");
            }
            catch (InvalidCastException ex)
            {
                _logger.LogError(ex, "Type conversion failed for operation {Operation}", operationName);
                return Result<T>.Failure($"Invalid response format: {ex.Message}");
            }
            catch (PythonRuntimeException ex)
            {
                _logger.LogError(ex, "Python runtime error in operation {Operation}", operationName);
                return Result<T>.Failure($"Python runtime error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in operation {Operation}", operationName);
                return Result<T>.Failure($"Unexpected error: {ex.Message}");
            }
        }
        
        return Result<T>.Failure($"Operation failed after {maxRetries} retries");
    }
    
    private static bool IsRetriableError(PythonInvocationException ex)
    {
        // Define which Python errors are worth retrying
        return ex.PythonExceptionType switch
        {
            "TimeoutError" => true,
            "ConnectionError" => true,
            "TemporaryFailure" => true,
            "ResourceTemporarilyUnavailable" => true,
            _ => false
        };
    }
}

// Result class for robust error handling
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; private set; }
    public string Error { get; private set; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

### Circuit Breaker Pattern

```csharp
public class CircuitBreakerPythonService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<CircuitBreakerPythonService> _logger;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly int _failureThreshold = 5;
    private readonly TimeSpan _recoveryTimeout = TimeSpan.FromMinutes(1);
    
    public T ExecuteWithCircuitBreaker<T>(Func<T> operation)
    {
        if (IsCircuitOpen())
        {
            throw new InvalidOperationException("Circuit breaker is open - Python operations temporarily disabled");
        }
        
        try
        {
            var result = operation();
            ResetCircuitBreaker();
            return result;
        }
        catch (Exception)
        {
            RecordFailure();
            throw;
        }
    }
    
    private bool IsCircuitOpen()
    {
        if (_failureCount >= _failureThreshold)
        {
            if (DateTime.Now - _lastFailureTime < _recoveryTimeout)
            {
                return true; // Circuit is open
            }
            else
            {
                // Try to recover
                _logger.LogInformation("Attempting to recover circuit breaker");
                return false;
            }
        }
        
        return false;
    }
    
    private void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.Now;
        
        if (_failureCount >= _failureThreshold)
        {
            _logger.LogWarning("Circuit breaker opened after {FailureCount} failures", _failureCount);
        }
    }
    
    private void ResetCircuitBreaker()
    {
        if (_failureCount > 0)
        {
            _logger.LogInformation("Circuit breaker reset - Python operations restored");
            _failureCount = 0;
            _lastFailureTime = DateTime.MinValue;
        }
    }
}
```

## Debugging and Diagnostics

### Enhanced Error Logging

```csharp
public class DiagnosticPythonService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<DiagnosticPythonService> _logger;
    
    public T ExecuteWithDiagnostics<T>(
        string operationName,
        Func<T> operation,
        object inputData = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationId = Guid.NewGuid().ToString("N")[..8];
        
        _logger.LogDebug("Starting operation {OperationName} [{OperationId}] with input: {Input}",
            operationName, operationId, inputData);
        
        try
        {
            var result = operation();
            stopwatch.Stop();
            
            _logger.LogInformation("Operation {OperationName} [{OperationId}] completed in {ElapsedMs}ms",
                operationName, operationId, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (PythonInvocationException ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Python operation {OperationName} [{OperationId}] failed after {ElapsedMs}ms\n" +
                "Python Exception: {PythonType}\n" +
                "Python Message: {PythonMessage}\n" +
                "Input Data: {InputData}\n" +
                "Python Stack Trace:\n{PythonStackTrace}\n" +
                "Python Locals: {PythonLocals}",
                operationName, operationId, stopwatch.ElapsedMilliseconds,
                ex.PythonExceptionType, ex.Message, inputData,
                ex.PythonStackTrace, FormatPythonVariables(ex.PythonLocals));
            
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex,
                "Operation {OperationName} [{OperationId}] failed after {ElapsedMs}ms with unexpected error",
                operationName, operationId, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
    
    private string FormatPythonVariables(Dictionary<string, PyObject> variables)
    {
        if (variables == null || !variables.Any())
            return "None";
        
        var formatted = variables.Select(kvp =>
        {
            try
            {
                return $"{kvp.Key}: {kvp.Value}";
            }
            catch
            {
                return $"{kvp.Key}: <unable to format>";
            }
        });
        
        return string.Join(", ", formatted);
    }
}
```

### Health Checks

```csharp
public class PythonHealthCheck : IHealthCheck
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<PythonHealthCheck> _logger;
    
    public PythonHealthCheck(IPythonEnvironment python, ILogger<PythonHealthCheck> logger)
    {
        _python = python;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test basic Python functionality
            var testResult = await Task.Run(() =>
            {
                var result = _python.ExecuteExpression("2 + 2");
                return result.As<long>();
            }, cancellationToken);
            
            if (testResult != 4)
            {
                return HealthCheckResult.Unhealthy("Python basic arithmetic test failed");
            }
            
            // Test module loading (if you have a health check module)
            try
            {
                var healthModule = _python.HealthCheck();
                var status = healthModule.GetStatus();
                
                return HealthCheckResult.Healthy($"Python runtime operational. Status: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check module not available, but basic Python works");
                return HealthCheckResult.Healthy("Python runtime operational (basic test only)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Python health check failed");
            return HealthCheckResult.Unhealthy($"Python runtime error: {ex.Message}");
        }
    }
}

// Registration
builder.Services.AddHealthChecks()
    .AddCheck<PythonHealthCheck>("python");
```

## Common Error Scenarios

### Import Errors

**Scenario:** Python module not found

```python
# my_module.py
import nonexistent_package  # This will cause ImportError

def my_function() -> str:
    return "Hello"
```

**Error Handling:**
```csharp
try
{
    var module = env.MyModule();
    var result = module.MyFunction();
}
catch (PythonInvocationException ex) when (ex.PythonExceptionType == "ImportError")
{
    _logger.LogError("Python import error: {Message}", ex.Message);
    
    // Provide helpful guidance
    throw new ApplicationException(
        "Required Python package is not installed. Please check requirements.txt and virtual environment setup.",
        ex);
}
```

### Type Annotation Mismatches

**Scenario:** Python function returns different type than annotated

```python
def get_number() -> int:
    return "not a number"  # Returns string instead of int
```

**Error Handling:**
```csharp
try
{
    long number = module.GetNumber();
}
catch (InvalidCastException ex)
{
    _logger.LogError("Type mismatch in Python function: {Message}", ex.Message);
    
    // Try to get the actual value
    using var result = env.ExecuteExpression("get_number()");
    var actualType = result.GetPythonType();
    
    throw new ApplicationException(
        $"Python function returned {actualType} but expected int",
        ex);
}
```

### Memory Issues

**Scenario:** Large Python objects causing memory pressure

```csharp
public class MemoryAwarePythonService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryAwarePythonService> _logger;
    
    public T ExecuteWithMemoryMonitoring<T>(Func<T> operation)
    {
        var beforeMemory = GC.GetTotalMemory(false);
        
        try
        {
            var result = operation();
            
            var afterMemory = GC.GetTotalMemory(false);
            var memoryUsed = afterMemory - beforeMemory;
            
            if (memoryUsed > 100_000_000) // 100MB threshold
            {
                _logger.LogWarning("High memory usage detected: {MemoryUsed} bytes", memoryUsed);
                
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            
            return result;
        }
        catch (OutOfMemoryException ex)
        {
            _logger.LogError(ex, "Out of memory during Python operation");
            
            // Emergency cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            throw new ApplicationException("Python operation exceeded available memory", ex);
        }
    }
}
```

## Error Recovery Strategies

### Automatic Retry with Backoff

```csharp
public class RetryablePythonService
{
    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<T> operation,
        RetryPolicy policy = null)
    {
        policy ??= RetryPolicy.Default;
        var attempt = 0;
        
        while (attempt < policy.MaxAttempts)
        {
            try
            {
                return operation();
            }
            catch (Exception ex) when (policy.ShouldRetry(ex, attempt))
            {
                attempt++;
                
                if (attempt < policy.MaxAttempts)
                {
                    var delay = policy.GetDelay(attempt);
                    await Task.Delay(delay);
                }
            }
        }
        
        throw new InvalidOperationException($"Operation failed after {policy.MaxAttempts} attempts");
    }
}

public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public Func<Exception, int, bool> ShouldRetry { get; set; }
    
    public static RetryPolicy Default => new()
    {
        ShouldRetry = (ex, attempt) => ex is PythonInvocationException pyEx &&
            (pyEx.PythonExceptionType == "TimeoutError" ||
             pyEx.PythonExceptionType == "ConnectionError")
    };
    
    public TimeSpan GetDelay(int attempt)
    {
        return TimeSpan.FromMilliseconds(
            BaseDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attempt - 1));
    }
}
```

### Fallback Mechanisms

```csharp
public class FallbackPythonService
{
    private readonly IPythonEnvironment _python;
    private readonly IFallbackService _fallback;
    
    public T ExecuteWithFallback<T>(
        Func<T> primaryOperation,
        Func<T> fallbackOperation)
    {
        try
        {
            return primaryOperation();
        }
        catch (PythonInvocationException ex)
        {
            _logger.LogWarning(ex, "Primary Python operation failed, using fallback");
            
            try
            {
                return fallbackOperation();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Fallback operation also failed");
                throw new AggregateException("Both primary and fallback operations failed", ex, fallbackEx);
            }
        }
    }
}
```

## Next Steps

- [Review API reference](api.md)
- [Learn about configuration](configuration.md)
- [Explore type mapping](type-mapping.md)
