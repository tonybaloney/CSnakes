using CommunityToolkit.HighPerformance;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;

public interface IPyBuffer : IDisposable
{
    /// <summary>
    /// The bit length of the buffer.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// The number of dimensions in the buffer. For a scalar, this is 1.
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// Indicates if the buffer is a scalar (single dimension array).
    /// </summary>
    bool IsScalar { get; }

    /// <summary>
    /// Gets the item type of the values in the buffer.
    /// </summary>
    [Obsolete($"Use '{nameof(IPyBuffer)}.{nameof(ItemType)}' instead.")]
    Type GetItemType() => ItemType;

    /// <summary>
    /// Gets the item type of the values in the buffer.
    /// </summary>
    Type ItemType { get; }

    /// <summary>
    /// Determines whether the buffer is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    Span<T> AsSpan<T>() where T : unmanaged;

    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged;

    [Obsolete($"Cast to '{nameof(PyArray2DBuffer<>)}' and use its '{nameof(PyArray2DBuffer<>.Do)}' method instead.")]
    Span2D<T> AsSpan2D<T>() where T : unmanaged;

    [Obsolete($"Cast to '{nameof(PyArray2DBuffer<>)}' and use its '{nameof(PyArray2DBuffer<>.Map)}' method instead.")]
    ReadOnlySpan2D<T> AsReadOnlySpan2D<T>() where T : unmanaged;

#if NET9_0_OR_GREATER
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}' and use its '{nameof(PyTensorBuffer<>.Do)}' method instead.")]
    TensorSpan<T> AsTensorSpan<T>() where T : unmanaged;

    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>() where T : unmanaged;
#endif // NET9_0_OR_GREATER
}
