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
    [Obsolete($"Use {nameof(IPyBuffer)}.{nameof(ItemType)} instead.")]
    Type GetItemType() => ItemType;

    /// <summary>
    /// Gets the item type of the values in the buffer.
    /// </summary>
    Type ItemType { get; }

    /// <summary>
    /// Determines whether the buffer is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    [Obsolete]
    Span<T> AsSpan<T>() where T : unmanaged;

    [Obsolete]
    ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged;

    [Obsolete]
    Span2D<T> AsSpan2D<T>() where T : unmanaged;

    [Obsolete]
    ReadOnlySpan2D<T> AsReadOnlySpan2D<T>() where T : unmanaged;

#if NET9_0_OR_GREATER
    [Obsolete]
    TensorSpan<T> AsTensorSpan<T>() where T : unmanaged;
    [Obsolete]
    ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>() where T : unmanaged;
#endif // NET9_0_OR_GREATER
}
