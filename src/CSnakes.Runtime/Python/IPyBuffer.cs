using CommunityToolkit.HighPerformance;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;
public interface IPyBuffer
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
    /// <returns></returns>
    Type GetItemType();

    Span<T> AsSpan<T>() where T : unmanaged;
    ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged;

    Span2D<T> AsSpan2D<T>() where T : unmanaged;
    ReadOnlySpan2D<T> AsReadOnlySpan2D<T>() where T : unmanaged;


#if NET9_0_OR_GREATER
    TensorSpan<T> AsTensorSpan<T>() where T : unmanaged;
    ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>() where T : unmanaged;
#endif

}
