# Buffer Protocol and NumPy Arrays

CSnakes supports Python objects that implement the buffer protocol, including `bytes`, `bytearray`, and NumPy arrays. Generated bindings return `CSnakes.Runtime.Python.IPyBuffer`, and the runtime chooses a more specific buffer type based on the buffer's element type and dimensionality.

In v2, buffer access is designed around `Map` and `Do` callbacks instead of span-returning APIs. If you are upgrading from v1.x and want the background for that change, see [Migration from v1.x to v2](#migration-from-v1x-to-v2).

The primary APIs are:

- `Map` gives read-only access for the duration of a computation and returns a result.
- `Do` gives writable access for the duration of an operation.
- `CopyTo` and `CopyFrom` let you safely move data between Python-owned memory and managed memory.

!!! danger
	Do not return or cache spans obtained from a Python buffer. Use `Map`, `Do`, `CopyTo`, or `CopyFrom` instead.

## Buffer Types

The runtime creates one of these buffer types when possible:

- `PyArrayBuffer<T>` for scalars and 1D buffers
- `PyArray2DBuffer<T>` for 2D buffers
- `PyTensorBuffer<T>` for 3D and higher-dimensional buffers on .NET 9 or later

Consumers typically receive `IPyBuffer`, then inspect or cast it:

```csharp
var buffer = testModule.ExampleArray();

Console.WriteLine(buffer.ItemType);   // typeof(float)
Console.WriteLine(buffer.Dimensions); // 1
Console.WriteLine(buffer.IsScalar);   // true
Console.WriteLine(buffer.IsReadOnly); // false for writable NumPy arrays

if (buffer is PyArrayBuffer<float> values)
    Console.WriteLine(values[0]);
```

## One-Dimensional Buffers

Use `PyArrayBuffer<T>` for scalar and 1D buffers.

```python
from collections.abc import Buffer

import numpy as np

def example_array() -> Buffer:
    return np.array([1.0, 2.0, 3.0, 4.0, 5.0], dtype=np.float32)
```

### Read values with an indexer

```csharp
using var buffer = (PyArrayBuffer<float>)testModule.ExampleArray();

Console.WriteLine(buffer[0]); // 1.0
Console.WriteLine(buffer[4]); // 5.0
```

### Compute with `Map`

`Map` is the preferred read-only API. It passes a `ReadOnlySpan<T>` to your callback and returns the callback result.

```csharp
using var buffer = (PyArrayBuffer<float>)testModule.ExampleArray();

var sum = buffer.Map(TensorPrimitives.Sum);
var firstIndexOverThree = buffer.Map(span => span.IndexOfAnyExcept(1f, 2f, 3f));
```

`Map` also has overloads that let you pass extra arguments into the callback:

```csharp
using var buffer = (PyArrayBuffer<float>)testModule.ExampleArray();

float[] weights = [5, 4, 3, 2, 1];
var dot = buffer.Map(weights, static (span, in other) => TensorPrimitives.Dot(span, other));
```

On .NET 9 or later, those extra arguments can also be ref structs such as `ReadOnlySpan<T>`.

### Modify in place with `Do`

`Do` is the writable counterpart to `Map`. It passes a `Span<T>` to your callback and throws if the buffer is read-only.

```csharp
using var buffer = (PyArrayBuffer<float>)testModule.ExampleArray();

buffer.Do(span => TensorPrimitives.Negate(span, span));
buffer.Do(3.0f, static (span, in factor) => TensorPrimitives.Multiply(span, factor, span));
```

### Copy data in or out

Use `CopyTo` and `CopyFrom` when you want a managed copy or when you need to populate the Python-backed buffer from managed data.

```csharp
using var buffer = (PyArrayBuffer<float>)testModule.ExampleArray();

float[] copy = new float[5];
buffer.CopyTo(copy);

float[] replacement = [10, 20, 30, 40, 50];
buffer.CopyFrom(replacement);
```

## Two-Dimensional Buffers

Use `PyArray2DBuffer<T>` for 2D buffers. Its callback-based APIs work the same way as `PyArrayBuffer<T>`, but operate on `Span2D<T>` and `ReadOnlySpan2D<T>`.

```python
def example_array_2d() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.float32)
```

```csharp
using var buffer = (PyArray2DBuffer<float>)testModule.ExampleArray2D();

Console.WriteLine(buffer[0, 0]); // 1
Console.WriteLine(buffer[1, 2]); // 6

var rowSums = buffer.Map(static (in span) =>
{
    var sums = new float[span.Height];
    for (var row = 0; row < span.Height; row++)
    {
        float total = 0;
        for (var col = 0; col < span.Width; col++)
            total += span[row, col];
        sums[row] = total;
    }
    return sums;
});

buffer.Do(static (in span) => span.Clear());
```

You can copy a 2D buffer either into a flat `Span<T>` or another `Span2D<T>`:

```csharp
using var buffer = (PyArray2DBuffer<float>)testModule.ExampleArray2D();

float[] flat = new float[6];
buffer.CopyTo(flat);
```

## N-dimensional Buffers on .NET 9+

Use `PyTensorBuffer<T>` for tensors with three or more dimensions. Its safe APIs mirror the other buffer types and operate on `TensorSpan<T>` and `ReadOnlyTensorSpan<T>`.

```python
def example_tensor() -> Buffer:
    arr = np.zeros((2, 3, 4, 5), dtype=np.int32)
    arr[0, 0, 0, 0] = 1
    arr[1, 2, 3, 4] = 3
    return arr
```

```csharp
using var buffer = (PyTensorBuffer<int>)testModule.ExampleTensor();

Console.WriteLine(buffer.Lengths.Length); // 4
Console.WriteLine(buffer[0, 0, 0, 0]);   // 1
Console.WriteLine(buffer[1, 2, 3, 4]);   // 3
```

For computations and updates, prefer `Map` and `Do`:

```csharp
using var buffer = (PyTensorBuffer<float>)testModule.ExampleTensor();

var first = buffer.Map(static (in span) => span[0, 0, 0, 0]);
buffer.Do(static (in span) => span[0, 0, 0, 0] = 255f);
```

## Read-only Buffers

Use `IsReadOnly` to decide whether mutation is allowed.

- `Map` is allowed on read-only buffers.
- `CopyTo` is allowed on read-only buffers.
- Indexer setters throw on read-only buffers.
- `Do` throws on read-only buffers.
- `CopyFrom` throws on read-only buffers.

This is enforced consistently across `PyArrayBuffer<T>`, `PyArray2DBuffer<T>`, and `PyTensorBuffer<T>`.

## Bytes and Bytearray Buffers

`bytes` and `bytearray` also participate in the buffer protocol.

- `bytes` is read-only, so use `Map` or `CopyTo`.
- `bytearray` is writable, so `Do`, `CopyFrom`, and indexer setters are also valid.

```csharp
using var buffer = (PyArrayBuffer<byte>)testModule.ExampleBytes();

var text = buffer.Map(span => System.Text.Encoding.UTF8.GetString(span));
```

## NumPy Type Conversion

CSnakes maps native-order NumPy dtypes to C# element types when it creates a typed buffer. Common mappings include:

| NumPy dtype | C# type |
|-------------|---------|
| `bool` | `bool` |
| `int8` | `sbyte` |
| `int16` | `short` |
| `int32` | `int` |
| `int64` | `long` |
| `uint8` | `byte` |
| `uint16` | `ushort` |
| `uint32` | `uint` |
| `uint64` | `ulong` |
| `float16` | `Half` |
| `float32` | `float` |
| `float64` | `double` |

If the runtime cannot safely create a typed buffer, you may still receive a more general `IPyBuffer` implementation instead of one of the typed shape-specific buffer classes.

## Migration from v1.x to v2

The buffer API in v2 is intentionally more explicit about lifetime and mutability. The safe replacement for most v1.x code is not "find the new span-returning method" but "move the work inside `Map` or `Do`".

This change exists because v1.x made it too easy to return a span from a disposable buffer object. That span could outlive the Python buffer that owned the underlying memory, which could then lead to access violations or memory corruption.

### Quick mapping

| v1.x | v2 |
|------|----|
| `buffer.GetItemType()` | `buffer.ItemType` |
| `buffer.AsSpan<T>()` | Cast to `PyArrayBuffer<T>` and use `Map`, `Do`, `CopyTo`, `CopyFrom`, or the indexer |
| `buffer.AsBoolSpan()` and similar extension methods | Cast to the right typed buffer and use its APIs |
| `buffer.AsSpan2D<T>()` | Cast to `PyArray2DBuffer<T>` and use `Map`, `Do`, `CopyTo`, `CopyFrom`, or the 2D indexer |
| `buffer.AsTensorSpan<T>()` | Cast to `PyTensorBuffer<T>` and use `Map`, `Do`, `CopyTo`, `CopyFrom`, or the tensor indexer |
| "Return a span from a helper" | Keep the computation inside `Map` or copy the data into managed memory |

### Before and after

This is the pattern that issue #504 called out as fundamentally unsafe:

```csharp
Span<byte> GetBufferSpan()
{
    using var buffer = testModule.ExampleBytes();
    return buffer.AsSpan<byte>();
}
```

The span escapes the lifetime of `buffer`, so it can end up pointing to freed Python memory.

In v2, keep the operation inside the callback:

```csharp
int FindUnexpectedByte(byte expected)
{
    using var buffer = (PyArrayBuffer<byte>)testModule.ExampleBytes();
    return buffer.Map(span => span.IndexOfAnyExcept(expected));
}
```

If you really need data that survives after the buffer is disposed, copy it:

```csharp
byte[] CopyBuffer()
{
    using var buffer = (PyArrayBuffer<byte>)testModule.ExampleBytes();
    var copy = new byte[buffer.Map(span => span.Length)];
    buffer.CopyTo(copy);
    return copy;
}
```

## Advanced Unsafe Escape Hatches

Most code should not need these APIs.

- `PyArrayBuffer<T>.UnsafeMemory` exposes the underlying memory directly.
- `PyTensorBuffer<T>.UnsafeAsTensorSpan()` exposes a tensor span directly.

Both are tied to the lifetime of the buffer object. Using them after disposal can corrupt memory or crash the process.

## Handling Non-contiguous Arrays

CSnakes expects a C-contiguous buffer layout when converting to the typed buffer APIs. Non-contiguous arrays, such as transposed NumPy arrays, can fail conversion.

Use [`np.ascontiguousarray()`](https://numpy.org/doc/stable/reference/generated/numpy.ascontiguousarray.html) in Python before returning the value to .NET when needed.
