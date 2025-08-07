# Free-Threading Mode

Python 3.13 introduced a new feature called "free-threading mode" which allows the Python interpreter to run in a multi-threaded environment without the Global Interpreter Lock (GIL). This is a significant change to the Python runtime and can have a big impact on the performance of Python code running in a multi-threaded environment.

## Overview

The Global Interpreter Lock (GIL) has historically been a limitation in Python's ability to utilize multiple CPU cores effectively in multi-threaded applications. Free-threading mode removes this limitation, allowing Python code to run truly in parallel across multiple threads.

## Enabling Free-Threading in CSnakes

CSnakes supports free-threading mode, but it is disabled by default. To use free-threading you can use the `RedistributableLocator` from version Python 3.13 and request `freeThreaded` builds:

```csharp
var builder = Host.CreateApplicationBuilder();
var pb = builder.Services.WithPython()
  .WithHome(Environment.CurrentDirectory) // Path to your Python modules.
  .FromRedistributable("3.13", freeThreaded: true);
var app = builder.Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

## Requirements

- **Python 3.13 or later**: Free-threading is only available in Python 3.13+
- **Compatible libraries**: Most Python libraries need updates to work with free-threading
- **Testing required**: Thorough testing is essential as this is still experimental

## Important Considerations

Whilst free-threading mode is **supported** at a high-level from CSnakes, it is still an experimental feature in Python 3.13 and may not be suitable for all use-cases. Also, most Python libraries, especially those written in C, are not yet compatible with free-threading mode, so you may need to test your code carefully.

## Benefits of Free-Threading

### CPU-Bound Tasks

Free-threading can significantly improve performance for CPU-bound Python operations:

```python
# cpu_intensive.py
import time
from concurrent.futures import ThreadPoolExecutor

def cpu_bound_task(n: int) -> int:
    """Simulate CPU-intensive work"""
    result = 0
    for i in range(n * 1000000):
        result += i * i
    return result

def parallel_computation(task_count: int, work_size: int) -> list[int]:
    """Run multiple CPU-bound tasks in parallel"""
    with ThreadPoolExecutor(max_workers=4) as executor:
        futures = [executor.submit(cpu_bound_task, work_size) for _ in range(task_count)]
        return [future.result() for future in futures]
```

```csharp
// With free-threading, this can utilize multiple CPU cores
var cpuModule = env.CpuIntensive();
var results = cpuModule.ParallelComputation(8, 100);
Console.WriteLine($"Completed {results.Count} parallel tasks");
```

### Mathematical Operations

```python
# math_parallel.py
import numpy as np
from concurrent.futures import ThreadPoolExecutor

def matrix_multiply(size: int) -> float:
    """Heavy matrix multiplication"""
    a = np.random.rand(size, size)
    b = np.random.rand(size, size)
    c = np.dot(a, b)
    return float(np.sum(c))

def parallel_matrix_ops(operations: int, matrix_size: int) -> list[float]:
    """Perform multiple matrix operations in parallel"""
    with ThreadPoolExecutor(max_workers=4) as executor:
        futures = [executor.submit(matrix_multiply, matrix_size) for _ in range(operations)]
        return [future.result() for future in futures]
```

## Library Compatibility

### Compatible Libraries

As of Python 3.13, these libraries have varying levels of free-threading support:

- **Pure Python libraries**: Generally work with free-threading
- **Standard library**: Most modules are compatible
- **Math/computation**: Basic math operations work

### Libraries Requiring Updates

Many popular libraries still need updates for free-threading:

- **NumPy**: Partial support, actively being developed
- **Pandas**: Limited support
- **SQLAlchemy**: Being updated
- **Requests**: Works for basic operations
- **Most C extensions**: Need explicit free-threading support

### Checking Compatibility

```python
# compatibility_check.py
import sys
import threading

def check_free_threading() -> dict[str, any]:
    """Check if free-threading is enabled and working"""
    return {
        "free_threading_enabled": hasattr(sys, '_is_gil_enabled') and not sys._is_gil_enabled(),
        "python_version": sys.version,
        "thread_count": threading.active_count(),
        "supports_parallel": True  # Will test this with actual parallel work
    }

def test_parallel_execution() -> dict[str, float]:
    """Test if parallel execution actually works"""
    import time
    from concurrent.futures import ThreadPoolExecutor
    
    def work(duration):
        start = time.time()
        # Simulate CPU work
        while time.time() - start < duration:
            pass
        return time.time() - start
    
    # Test sequential vs parallel
    start_sequential = time.time()
    for _ in range(4):
        work(0.1)
    sequential_time = time.time() - start_sequential
    
    start_parallel = time.time()
    with ThreadPoolExecutor(max_workers=4) as executor:
        futures = [executor.submit(work, 0.1) for _ in range(4)]
        [future.result() for future in futures]
    parallel_time = time.time() - start_parallel
    
    return {
        "sequential_time": sequential_time,
        "parallel_time": parallel_time,
        "speedup": sequential_time / parallel_time if parallel_time > 0 else 0
    }
```

```csharp
var compatibility = env.CompatibilityCheck();
var status = compatibility.CheckFreeThreading();
var performance = compatibility.TestParallelExecution();

Console.WriteLine($"Free-threading enabled: {status["free_threading_enabled"]}");
Console.WriteLine($"Speedup achieved: {performance["speedup"]:F2}x");
```

## Performance Comparison

### Traditional GIL vs Free-Threading

```python
# performance_test.py
import time
import threading
from concurrent.futures import ThreadPoolExecutor

def cpu_work(iterations: int) -> float:
    """CPU-intensive work to test threading"""
    start = time.time()
    total = 0
    for i in range(iterations):
        total += i ** 2
    return time.time() - start

def compare_threading_modes(work_amount: int, thread_count: int) -> dict[str, float]:
    """Compare sequential vs threaded performance"""
    
    # Sequential execution
    start = time.time()
    for _ in range(thread_count):
        cpu_work(work_amount)
    sequential_time = time.time() - start
    
    # Threaded execution
    start = time.time()
    with ThreadPoolExecutor(max_workers=thread_count) as executor:
        futures = [executor.submit(cpu_work, work_amount) for _ in range(thread_count)]
        [future.result() for future in futures]
    threaded_time = time.time() - start
    
    return {
        "sequential_time": sequential_time,
        "threaded_time": threaded_time,
        "speedup": sequential_time / threaded_time,
        "efficiency": (sequential_time / threaded_time) / thread_count
    }
```

## Best Practices

### 1. Test Thoroughly

```csharp
public class FreeThreadingValidator
{
    private readonly IPythonEnvironment _env;
    
    public async Task<bool> ValidatePerformanceGains()
    {
        var perfTest = _env.PerformanceTest();
        var results = perfTest.CompareThreadingModes(1000000, 4);
        
        var speedup = results["speedup"].As<double>();
        return speedup > 1.5; // Expect at least 50% improvement
    }
}
```

### 2. Monitor Resource Usage

```csharp
public class ResourceMonitor
{
    public void MonitorDuringExecution(Action workload)
    {
        var beforeMemory = GC.GetTotalMemory(false);
        var beforeCpu = Environment.TickCount;
        
        workload();
        
        var afterMemory = GC.GetTotalMemory(true);
        var afterCpu = Environment.TickCount;
        
        Console.WriteLine($"Memory used: {afterMemory - beforeMemory:N0} bytes");
        Console.WriteLine($"CPU time: {afterCpu - beforeCpu} ms");
    }
}
```

### 3. Gradual Migration

Start with specific modules or operations that benefit most from parallelization:

```python
# gradual_migration.py
def parallel_safe_operation(data: list[int]) -> list[int]:
    """Operation that's safe for free-threading"""
    from concurrent.futures import ThreadPoolExecutor
    
    def process_chunk(chunk):
        return [x * 2 for x in chunk]
    
    chunk_size = len(data) // 4
    chunks = [data[i:i+chunk_size] for i in range(0, len(data), chunk_size)]
    
    with ThreadPoolExecutor(max_workers=4) as executor:
        results = list(executor.map(process_chunk, chunks))
    
    return [item for sublist in results for item in sublist]
```

## Limitations and Gotchas

### Memory Overhead

Free-threading can increase memory usage due to:
- Per-object reference counting overhead
- Thread-local storage requirements
- Additional synchronization structures

### Not Always Faster

Free-threading may not improve performance for:
- I/O-bound operations (already parallel with asyncio)
- Operations with significant synchronization overhead
- Single-threaded workloads

### Library Ecosystem

Many third-party libraries still need updates to fully support free-threading mode.

## Migration Strategy

1. **Test with Pure Python**: Start with pure Python code
2. **Validate Performance**: Measure actual performance gains
3. **Check Dependencies**: Verify all dependencies support free-threading
4. **Gradual Rollout**: Migrate module by module
5. **Monitor Production**: Watch for performance regressions

## Troubleshooting

### Common Issues

```python
# debug_free_threading.py
import sys
import threading

def diagnose_threading_issues() -> dict[str, any]:
    """Diagnose common free-threading issues"""
    issues = []
    
    # Check if free-threading is actually enabled
    if hasattr(sys, '_is_gil_enabled'):
        gil_enabled = sys._is_gil_enabled()
        if gil_enabled:
            issues.append("GIL is still enabled - free-threading not active")
    else:
        issues.append("Python version doesn't support free-threading detection")
    
    # Check thread count
    active_threads = threading.active_count()
    if active_threads == 1:
        issues.append("Only one thread active - may not be utilizing parallelism")
    
    return {
        "issues": issues,
        "gil_enabled": gil_enabled if 'gil_enabled' in locals() else None,
        "active_threads": active_threads,
        "python_version": sys.version
    }
```

## Next Steps

- [Explore manual Python integration](manual-integration.md)
- [Learn about hot reload](hot-reload.md)
- [Review performance optimization](performance.md)
