# Performance

**Optimal performance** is one of the key design principles of CSnakes. That
said, there are some important differences between Python and .NET which impact
performance when embedding.

This page documents some performance considerations when using CSnakes.

## Important Concepts

- **Marshalling** refers to the transfer of data and types between platforms
  (Python to .NET or .NET to Python). For example converting a Python `int` into
  a C#.NET `long`.

## 3 Things to avoid when performance matters

### 1. "Crossing the bridge" too frequently

Python and .NET functions have very different calling conventions. Whilst every
effort has been made to make calling Python functions fast from .NET in CSnakes,
function calls are relatively slow compared with regular .NET to .NET calls.
Function calling in Python is _generally_ slow (compared to C#, C++, Rust, C,
etc.) so when writing performance Python code you should avoid writing tiny
functions and making too many calls. Python doesn't have function inlining
(unlike C# with JIT or AOT compilation) and a lot of CPU time is spent creating
call frames.

Calling Python functions from CSnakes is slower than calling Python functions
from Python, because CSnakes has to marshal the input values from .NET into
Python objects and vice versa. Therefore, you want to avoid "crossing the
bridge" (going between .NET and Python) too frequently.

For example, take this code:

```python
import numpy as np

def make_square_2d_array(n: int) -> np.ndarray:
    return np.zeros((n, n))

def set_random(arr, i: int, j: int) -> None:
    arr[i, j] = np.random.random()
```

From C#, you fill or set the values in the array like this:

```csharp
var names = env.Make2dArray(1000);

for (int i = 0; i < 1000; i++)
{
    for (int j = 0; j < 1000; j++)
    {
        env.SetRandom(names, i, j);
    }
}
```

This is a **very inefficient** way to set the values in the array. The
`SetRandom` function is called 1,000,000 times and each time it crosses the
bridge between .NET and Python. For all 1,000,000 calls, the C# integers `i` and
`j` are converted to Python integers, the function is called, and then the
result is converted back.

Instead, you should design your code with a wrapper function to minimize the
number of interop calls. For example, you could create a function that sets all
the values in the array at once:

```python
def set_random(arr: np.ndarray) -> None:
    for i in range(arr.shape[0]):
        for j in range(arr.shape[1]):
            arr[i, j] = np.random.random()
```

### 2. Marshalling return values unnecessarily

Unlike .NET which has value types and reference types, Python has only names
which are references to objects (everything is a pointer to `PyObject`). If you're a C# developer, just think of Python
as having only reference types. 

To marshall value types from .NET to Python, CSnakes has to convert the value
type into a reference type. This is done by creating a new object in Python and
copying the value into it. This is a relatively expensive operation and should
be avoided when possible. There are some performance tricks in CSnakes to intern
certain values like `0`, `1`, `True`, `False`, and `None` to avoid this
overhead, but you should still be careful when passing value types to Python
functions.

If you don't need to read the return value of a Python function, you can hint
the function as `Any`. This will tell the source generator to return a
`PyObject`, which is a `SafeHandle` to the Python object. This will avoid the
overhead of marshalling the return value into a .NET type.

This is particularly useful when passing data between functions.

For example, if the Python function `a` returns an object and you need that to
pass to another function `b`, you can do this:

```python
def a():
    return ...

def b(x: ...) -> None:
    # Do something with x
    pass
```

Instead of marshalling the return value of `a` into a .NET type, you can use
`Any` to avoid the overhead:

```python
def a() -> Any:
    return ...
```

Then from C#, you use the `PyObject` type to pass the value to the next
function:

```csharp
using CSnakes.Runtime;

var pyObj = env.A();
env.B(pyObj);
```

#### Tuples are value types

In .NET, Tuple types are value types; tuple elements are public fields. Unlike
Python, where tuples are immutable and returned as a reference (pointer to `PyObject`). This means that if
a Python object returns a tuple, each of the elements in the tuple is eagerly
marshalled into the corresponding .NET type.

That is different to dictionaries and lists, which are lazily marshalled (see
below).

#### Lazy dictionaries

If a Python function returns a dictionary, CSnakes will return
`IReadOnlyDictionary` with an implementation to lazily convert the values to
.NET types. The conversion is completed the first time the key value is accessed. Lazy Conversion is done to avoid the overhead of converting all the values in
the dictionary to .NET types when the dictionary is created.

```csharp
IReadOnlyDictionary<string, int> dict = env.ExampleDict();

// Don't do this to get the value of a key
foreach (var kvp in dict)
{
    if (kvp.Key == "key")
    {
        // Do something with the value
        int value = kvp.Value;
    }
}

// Instead, check the existence of a key
if (dict.ContainsKey("key"))
{
    // Get the value of the key
    int value = dict["key"];
}
```

#### Lazy lists

Similar to dictionaries, if a Python function returns a list, CSnakes will
return `IReadOnlyList` with an implementation to lazily convert the values to
.NET types as each index is accessed.

Where possible, you should try and avoid iterating over the list to get a single
or a few values. Instead, you should use the `IReadOnlyList` interface to index
into the specific values you need.

If you do, the marshalled value is cached in the `IReadOnlyList` implementation.
This means that if you call the same index multiple times, the value is only
marshalled once.

### 3. Sending large amounts of data to Python

Whilst Python functions which return lists and dictionaries are lazily
marshalled, functions which take lists and dictionaries as arguments are not;
they are copied instead. This means that if you pass a large list or dictionary
to a Python function, CSnakes has to eagerly marshal the entire list or
dictionary into .NET types.

Take this example:

```python
def example_function(data: list[int]) -> None:
    # Do something with the data
    pass
```

From C#, you can call this function like this:

```csharp
env.ExampleFunction([1, 2, 3]);
```

When calling the Python function, CSnakes has to create a Python list object and
create a Python integer object for each element in the list. If there are
thousands or millions of elements in the list, this can be a very expensive
operation.

There are some alternatives:

#### Using bytes

If you are passing a large amount of data to Python, you can use a `bytes`
instead of a list. This will avoid the overhead of creating a Python list and
converting each element to a Python object.

```python
from array import array

def example_function(data: bytes) -> None:
    # Do something with the data
    arr = array('B', data) # unsigned char
    # Do something with the array
```

From C#, you can call this function like this:

```csharp
byte[] data = new byte[1_000_000];
for (int i = 0; i < data.Length; i++)  // Fill the byte array with data (example)
{
    data[i] = (byte)i;
}
env.ExampleFunction(data);
```

CSnakes only creates 1 Python object and copies the byte array into the bytes object. This is more efficient than creating an array or tuple of values, because each of the elements in the array needs to be created as a Python object and allocated. You can use the [array][array-mod]
module to convert the bytes into a list of an underlying C type.

#### Using numpy arrays

If you're sending large byte data, or numerical data from Python to .NET, you
should use the [Buffer protocol][buffer] to pass the data.

If you need to copy lots of numerical data from .NET to Python, we recommend
creating a numpy array in Python and using the [`Buffer` type][buffer] to fill
it from .NET. This is the fastest way to pass large amounts of data to Python.

You can combine generators with the `Buffer` type to `yield` the empty Numpy
array, fill it from .NET, the continue with execution in Python:

```python
import numpy as np
from typing import Generator
from collections.abc import Buffer

def sum_of_2d_array(n: int) -> Generator[Buffer, None, int]:
    arr = np.zeros((n, n), dtype=np.int32)
    yield arr
    return np.sum(arr).item()
```

From C#, you can call the generator, wait for the first yield, and then fill the
array with data from .NET:

```csharp
// whatever your data looks like, e.g. a list of Int32
List<Int32> list = new() { 1, 2, 3, 4, 5 };

var bufferGenerator = testModule.SumOf2dArray(5);

// Move to the first yield
bufferGenerator.MoveNext();
// Get the buffer object
var bufferObject = bufferGenerator.Current;
// Get the buffer as a 2D span of Int32
var bufferAsSpan = bufferObject.AsInt32Span2D();

// Copy the list to the buffer
for (int i = 0; i < list.Count; i++)
{
    for (int j = 0; j < list.Count; j++)
    {
        bufferAsSpan[i, j] = list[i];
    }
}
// Continue execution in Python
bufferGenerator.MoveNext();
// Get return value
long result = bufferGenerator.Return;
```

## Streaming data from Python into .NET

If you need to send lots of data from Python to .NET, you can either use the
[buffer protocol][buffer] or use a [generator] to stream the data from Python to
.NET.

[array-mod]: https://docs.python.org/3/library/array.html
[buffer]: ../user-guide/buffers.md
[generator]: ../reference/type-mapping.md#generators
