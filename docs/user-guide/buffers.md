# Buffer Protocol and NumPy Arrays

CSnakes supports the Python Buffer Protocol for `bytes` and `bytearray` types. The Buffer Protocol is a low-level interface for reading and writing raw bytes from Python objects.
The `IPyBuffer` interface is used to represent Python objects that support the Buffer Protocol. It has methods for accessing the raw data of the buffer in a Read-Only or Read-Write Span.

Since NumPy ndarrays also support the Buffer Protocol, you can use the `IPyBuffer` interface to efficiently read and write data from NumPy arrays.

`collections.abc.Buffer` was introduced in Python 3.12, but for older versions you can import `Buffer` from the `typing_extensions` package on PyPi.

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
Console.WriteLine(result2D[2, 3]); // 6

```

## N-dimensional Buffers (.NET 9)

In .NET 9, the `IPyBuffer` interface also provides methods for working with N-dimensional buffers using the Experimental [`System.Numerics.Tensors.TensorSpan` type](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.tensors.tensorspan-1?view=net-8.0). You can use the `AsTensorSpan` and `AsReadOnlyTensorSpan` methods to get a N-dimensional TensorSpan of the buffer contents.

For example:

```python
def example_tensor() -> Buffer:
    arr = np.zeros((2, 3, 4, 5), dtype=np.int32)
    arr[0, 0, 0, 0] = 1
    arr[1, 2, 3, 4] = 3
    return arr
```

From .NET you can access the buffer as a N-dimensional TensorSpan:

```csharp
// Where testModule is a Python module that contains the example_tensor function
var result = testModule.ExampleTensor();

// Get the buffer contents as a N-dimensional TensorSpan of int
TensorSpan<int> resultTensor = result.AsTensorSpan<int>(); // or AsInt32ReadOnlyTensorSpan
Console.WriteLine(resultTensor[0, 0, 0, 0]); // 1
Console.WriteLine(resultTensor[1, 2, 3, 4]); // 3
```

## NumPy Type Conversion

The Numpy dtypes are mapped to C# types as follows:

| NumPy dtype | C# Type  | Span Method    | ReadOnly Span Method   | Span2D Method    | ReadOnly Span2D Method   | TensorSpan Method    | ReadOnly TensorSpan Method   |
|-------------|----------|----------------|------------------------|------------------|--------------------------|----------------------|------------------------------|
| `bool`      | `bool`   | `AsBoolSpan`   | `AsBoolReadOnlySpan`   | `AsBoolSpan2D`   | `AsBoolReadOnlySpan2D`   | `AsBoolTensorSpan`   | `AsBoolReadOnlyTensorSpan`   |
| `int8`      | `sbyte`  | `AsSByteSpan`  | `AsSByteReadOnlySpan`  | `AsSByteSpan2D`  | `AsSByteReadOnlySpan2D`  | `AsSByteTensorSpan`  | `AsSByteReadOnlyTensorSpan`  |
| `int16`     | `short`  | `AsInt16Span`  | `AsInt16ReadOnlySpan`  | `AsInt16Span2D`  | `AsInt16ReadOnlySpan2D`  | `AsInt16TensorSpan`  | `AsInt16ReadOnlyTensorSpan`  |
| `int32`     | `int`    | `AsInt32Span`  | `AsInt32ReadOnlySpan`  | `AsInt32Span2D`  | `AsInt32ReadOnlySpan2D`  | `AsInt32TensorSpan`  | `AsInt32ReadOnlyTensorSpan`  |
| `int64`     | `long`   | `AsInt64Span`  | `AsInt64ReadOnlySpan`  | `AsInt64Span2D`  | `AsInt64ReadOnlySpan2D`  | `AsInt64TensorSpan`  | `AsInt64ReadOnlyTensorSpan`  |
| `uint8`     | `byte`   | `AsByteSpan`   | `AsByteReadOnlySpan`   | `AsByteSpan2D`   | `AsByteReadOnlySpan2D`   | `AsByteTensorSpan`   | `AsByteReadOnlyTensorSpan`   |
| `uint16`    | `ushort` | `AsUInt16Span` | `AsUInt16ReadOnlySpan` | `AsUInt16Span2D` | `AsUInt16ReadOnlySpan2D` | `AsUInt16TensorSpan` | `AsUInt16ReadOnlyTensorSpan` |
| `uint32`    | `uint`   | `AsUInt32Span` | `AsUInt32ReadOnlySpan` | `AsUInt32Span2D` | `AsUInt32ReadOnlySpan2D` | `AsUInt32TensorSpan` | `AsUInt32ReadOnlyTensorSpan` |
| `uint64`    | `ulong`  | `AsUInt64Span` | `AsUInt64ReadOnlySpan` | `AsUInt64Span2D` | `AsUInt64ReadOnlySpan2D` | `AsUInt64TensorSpan` | `AsUInt64ReadOnlyTensorSpan` |
| `float16`   | `Half`   | `AsHalfSpan`   | `AsHalfReadOnlySpan`   | `AsHalfSpan2D`   | `AsHalfReadOnlySpan2D`   | `AsHalfTensorSpan`   | `AsHalfReadOnlyTensorSpan`   |
| `float32`   | `float`  | `AsFloatSpan`  | `AsFloatReadOnlySpan`  | `AsFloatSpan2D`  | `AsFloatReadOnlySpan2D`  | `AsFloatTensorSpan`  | `AsFloatReadOnlyTensorSpan`  |
| `float64`   | `double` | `AsDoubleSpan` | `AsDoubleReadOnlySpan` | `AsDoubleSpan2D` | `AsDoubleReadOnlySpan2D` | `AsDoubleTensorSpan` | `AsDoubleReadOnlyTensorSpan` |

The `GetItemType()` method can be used to get the C# type of the buffer contents. 

You can also use generic methods such as `AsSpan<T>` and `AsReadOnlySpan<T>` to get a Span of the buffer contents with the specified type. If the requested type does not match the buffer contents, an exception will be thrown.

## Bytes objects as buffers

In addition to NumPy arrays, you can also use `bytes` and `bytearray` objects as buffers. The `Buffer` type hint can be used to indicate that a function returns a `bytes` or `bytearray` object that supports the Buffer Protocol.

The Buffer Protocol is an efficient way to read and write bytes between C# and Python. Use `AsByteSpan` and `AsByteReadOnlySpan` to access the raw bytes of the buffer.

## Handing non-contiguous arrays

If the NumPy array is not C-contiguous, the Buffer conversion throw an exception. This will happen for example when you transpose a NumPy array.

To convert a Fortran-contiguous array to a C-contiguous array, you can use the [`np.ascontiguousarray()` function](https://numpy.org/doc/stable/reference/generated/numpy.ascontiguousarray.html) in Python before returning the array to C#.
