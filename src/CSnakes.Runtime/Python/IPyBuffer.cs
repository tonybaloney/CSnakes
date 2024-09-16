using CommunityToolkit.HighPerformance;

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

    Span<bool> AsBoolSpan();
    Span<byte> AsByteSpan();
    Span<sbyte> AsSByteSpan();
    Span<short> AsInt16Span();
    Span<ushort> AsUInt16Span();
    Span<int> AsInt32Span();
    Span<long> AsInt64Span();
    Span<uint> AsUInt32Span();
    Span<ulong> AsUInt64Span();
    Span<float> AsFloatSpan();
    Span<double> AsDoubleSpan();
    Span<nint> AsIntPtrSpan();
    Span<nuint> AsUIntPtrSpan();

    ReadOnlySpan<bool> AsBoolReadOnlySpan();
    ReadOnlySpan<byte> AsByteReadOnlySpan();
    ReadOnlySpan<sbyte> AsSByteReadOnlySpan();
    ReadOnlySpan<short> AsInt16ReadOnlySpan();
    ReadOnlySpan<ushort> AsUInt16ReadOnlySpan();
    ReadOnlySpan<int> AsInt32ReadOnlySpan();
    ReadOnlySpan<long> AsInt64ReadOnlySpan();
    ReadOnlySpan<uint> AsUInt32ReadOnlySpan();
    ReadOnlySpan<ulong> AsUInt64ReadOnlySpan();
    ReadOnlySpan<float> AsFloatReadOnlySpan();
    ReadOnlySpan<double> AsDoubleReadOnlySpan();
    ReadOnlySpan<nint> AsIntPtrReadOnlySpan();
    ReadOnlySpan<nuint> AsUIntPtrReadOnlySpan();

    Span2D<bool> AsBoolSpan2D();
    Span2D<byte> AsByteSpan2D();
    Span2D<sbyte> AsSByteSpan2D();
    Span2D<short> AsInt16Span2D();
    Span2D<ushort> AsUInt16Span2D();
    Span2D<int> AsInt32Span2D();
    Span2D<uint> AsUInt32Span2D();
    Span2D<long> AsInt64Span2D();
    Span2D<ulong> AsUInt64Span2D();
    Span2D<float> AsFloatSpan2D();
    Span2D<double> AsDoubleSpan2D();
    Span2D<nint> AsIntPtrSpan2D();
    Span2D<nuint> AsUIntPtrSpan2D();

    ReadOnlySpan2D<bool> AsBoolReadOnlySpan2D();
    ReadOnlySpan2D<byte> AsByteReadOnlySpan2D();
    ReadOnlySpan2D<sbyte> AsSByteReadOnlySpan2D();
    ReadOnlySpan2D<short> AsInt16ReadOnlySpan2D();
    ReadOnlySpan2D<ushort> AsUInt16ReadOnlySpan2D();
    ReadOnlySpan2D<int> AsInt32ReadOnlySpan2D();
    ReadOnlySpan2D<uint> AsUInt32ReadOnlySpan2D();
    ReadOnlySpan2D<long> AsInt64ReadOnlySpan2D();
    ReadOnlySpan2D<ulong> AsUInt64ReadOnlySpan2D();
    ReadOnlySpan2D<float> AsFloatReadOnlySpan2D();
    ReadOnlySpan2D<double> AsDoubleReadOnlySpan2D();
    ReadOnlySpan2D<nint> AsIntPtrReadOnlySpan2D();
    ReadOnlySpan2D<nuint> AsUIntPtrReadOnlySpan2D();
}
