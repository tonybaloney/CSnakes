# Async support

Python async functions will be generated into async C# methods. The generated C# method will return a `Task<T>` depending on the return type of the Python function.

```python
import asyncio

async def async_function() -> int:
    await asyncio.sleep(1)
    return 42
```

The generated C# method will have this signature:


```csharp
public async Task<int> AsyncFunction(CancellationToken cancellationToken = default);
```

Python async functions can be awaited in C# code.

## Implementation Details

The [C# Async model](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) and the Python Async models have some important differences:

- C# creates a task pool and tasks are scheduled on this pool. Python uses a single-threaded event loop.
- Python event loops belong to the thread that created them. C# tasks can be scheduled on any thread.
- Python async functions are coroutines that are scheduled on the event loop. C# async functions are tasks that are scheduled on the task pool.

To converge these two models, CSnakes creates a Python event-loop that is services by a .NET thread and which is in turn used to schedule the Python async functions.

## Parallelism considerations

Event though C# uses a thread-pool to schedule tasks, the Python Global Interpreter Lock (GIL) will prevent multiple Python threads from running in parallel.
This means that even if you use parallel LINQ or other parallel constructs in C#, CPU-bound Python code will mostly run in a single thread at a time.

Python 3.13 and above have a feature called "free-threading mode" which allows the Python interpreter to run in a multi-threaded environment without the Global Interpreter Lock (GIL). This is a significant change to the Python runtime and can have a big impact on the performance of Python code running in a multi-threaded environment.
See [Free-Threading Mode](../advanced/free-threading.md) for more information.
