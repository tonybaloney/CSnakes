# Async support

Python async functions will be generated into async C# methods. The generated C# method will return a `Task<T>` depending on the return type of the Python function. 

CSnakes requires strict typing for async functions. This means that the return type of the Python function must be annotated with the `typing.Coroutine` type hint.

```python
import asyncio
from typing import Coroutine

async def async_function() -> Coroutine:
    await asyncio.sleep(1)
    return 42
```

The generated C# method will have this signature:


```csharp
public async Task<int> AsyncFunction();
```

Python async functions can be awaited in C# code.

## TSend and TResult

CSnakes only supports `Task<TYield>` where T is `Coroutine[TYield, ..., ...]`. You cannot send values back into the coroutine object.

## Implementation Details

The [C# Async model](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming) and the Python Async models have some important differences:

- C# creates a task pool and tasks are scheduled on this pool. Python uses a single-threaded event loop.
- Python event loops belong to the thread that created them. C# tasks can be scheduled on any thread.
- Python async functions are coroutines that are scheduled on the event loop. C# async functions are tasks that are scheduled on the task pool.

To converge these two models, CSnakes creates a Python event-loop for each C# thread that calls into Python. This event loop is created when the first Python function is called and is destroyed when the thread is disposed. This event loop is used to schedule the Python async functions.
Because C# reuses threads in the Task pool, the event loop is reused and kept as a thread-local variable.

The behavior is abstracted away from the user, but it is important to understand that the Python event loop is created and destroyed for each C# thread that calls into Python. This is important to understand when debugging or profiling your application.

## Parallelism considerations

Event though C# uses a thread-pool to schedule tasks, the Python Global Interpreter Lock (GIL) will prevent multiple Python threads from running in parallel.
This means that even if you use parallel LINQ or other parallel constructs in C#, CPU-bound Python code will mostly run in a single thread at a time.

Python 3.13 and above have a feature called "free-threading mode" which allows the Python interpreter to run in a multi-threaded environment without the Global Interpreter Lock (GIL). This is a significant change to the Python runtime and can have a big impact on the performance of Python code running in a multi-threaded environment.
See [Free-Threading Mode](advanced.md#free-threading-mode) for more information.
