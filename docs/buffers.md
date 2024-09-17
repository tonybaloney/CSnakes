# Buffer Protocol and NumPy Arrays

CSnakes supports the Python Buffer Protocol for `bytes` and `bytearray` types. The Buffer Protocol is a low-level interface for reading and writing raw bytes from Python objects.
The `IPyBuffer` interface is used to represent Python objects that support the Buffer Protocol. It has methods for accessing the raw data of the buffer in a Read-Only or Read-Write Span.

Since NumPy ndarrays also support the Buffer Protocol, you can use the `IPyBuffer` interface to efficiently read and write data from NumPy arrays.

`typing.Buffer` (`collections.abc.Buffer`) was introduced in Python 3.12, but for older versions you can import `Buffer` from the `typing_extensions` package on PyPi. 

For example:

```python
try:
    from collections.abc import Buffer
except ImportError:
    from typing_extensions import Buffer

import numpy as np

def example_array() -> Buffer:
    return np.array([True, False, True, False, False], dtype=np.bool_)
```

In this example, the `example_array` function returns a NumPy array of boolean values. The `Buffer` type hint indicates that the function returns an object that supports the Buffer Protocol.

From C#, CSnakes will return an `CSnakes.Runtime.Python.IPyBuffer` that provides access to the data as a `Span`.

For example:

```csharp
// Where testModule is a Python module that contains the example_array function
var bufferObject = testModule.ExampleArray();

// check if the buffer is scalar (single dimensional)
if (bufferObject.IsScalar) {

// Get the buffer contents as a Span of bool
	Span<bool> result = bufferObject.AsBoolSpan();
	Console.WriteLine(result[0]); // True
	Console.WriteLine(result[4]); // False
}
```

!!! danger

	`Span` is writeable so you can also modify the buffer contents from C# and the changes will be reflected in the Python object.
	If you have want a read-only view of the buffer, you can use the `As[T]ReadOnly` method to get a read-only Span.
	We recommend using the read-only methods when you don't need to modify the buffer contents.


If you want a read-only view of the buffer, you can use the `As[T]ReadOnly` method to get a read-only Span.

```csharp
Span<bool> result = bufferObject.AsBoolReadOnlySpan();
```

## Two-Dimensional Buffers

The `IPyBuffer` interface also provides methods for working with two-dimensional buffers. You can use the `As[T]Span2D` and `As[T]ReadOnlySpan2D` methods to get a two-dimensional Span of the buffer contents.

You can use the `Dimensions` property to get the dimensions of the buffer. The `As[T]Span2D` method will throw an exception if the buffer is not two-dimensional.

For example:

```python
def example_array_2d() -> Buffer:
	return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int32)
```

From .NET you can access the buffer as a two-dimensional Span:

```csharp
// Where testModule is a Python module that contains the example_array_2d function
var result = testModule.ExampleArray2D();

// Get the buffer contents as a two-dimensional Span of int
Span2D<int> result2D = result.AsIntSpan2D();
Console.WriteLine(result2D[0, 0]); // 1

```


## NumPy Type Conversion

The Numpy dtypes are mapped to C# types as follows:

| NumPy dtype | C# Type | Span Method | ReadOnly Span Method | Span2D Method | ReadOnly Span2D Method |
|-------------|---------|-------------|----------------------|---------------|------------------------|
| `bool`      | `bool`  | `AsBoolSpan`| `AsBoolReadOnlySpan` | `AsBoolSpan2D`| `AsBoolReadOnlySpan2D` |
| `int8`      | `sbyte` | `AsSByteSpan`| `AsSByteReadOnlySpan` | `AsSByteSpan2D`| `AsSByteReadOnlySpan2D` |
| `int16`     | `short` | `AsShortSpan`| `AsShortReadOnlySpan` | `AsShortSpan2D`| `AsShortReadOnlySpan2D` |
| `int32`     | `int`   | `AsIntSpan`  | `AsIntReadOnlySpan`   | `AsIntSpan2D`  | `AsIntReadOnlySpan2D`   |
| `int64`     | `long`  | `AsLongSpan` | `AsLongReadOnlySpan`  | `AsLongSpan2D` | `AsLongReadOnlySpan2D`  |
| `uint8`     | `byte`  | `AsByteSpan` | `AsByteReadOnlySpan`  | `AsByteSpan2D` | `AsByteReadOnlySpan2D`  |
| `uint16`    | `ushort`| `AsUShortSpan`| `AsUShortReadOnlySpan`| `AsUShortSpan2D`| `AsUShortReadOnlySpan2D`|
| `uint32`    | `uint`  | `AsUIntSpan` | `AsUIntReadOnlySpan`  | `AsUIntSpan2D` | `AsUIntReadOnlySpan2D`  |
| `uint64`    | `ulong` | `AsULongSpan`| `AsULongReadOnlySpan` | `AsULongSpan2D`| `AsULongReadOnlySpan2D` |
| `float32`   | `float` | `AsFloatSpan`| `AsFloatReadOnlySpan` | `AsFloatSpan2D`| `AsFloatReadOnlySpan2D` |
| `float64`   | `double`| `AsDoubleSpan`| `AsDoubleReadOnlySpan`| `AsDoubleSpan2D`| `AsDoubleReadOnlySpan2D`|

The `GetItemType()` method can be used to get the C# type of the buffer contents. 

## Bytes objects as buffers

In addition to NumPy arrays, you can also use `bytes` and `bytearray` objects as buffers. The `Buffer` type hint can be used to indicate that a function returns a `bytes` or `bytearray` object that supports the Buffer Protocol.

The Buffer Protocol is an efficient way to read and write bytes between C# and Python. Use `AsByteSpan` and `AsByteReadOnlySpan` to access the raw bytes of the buffer.