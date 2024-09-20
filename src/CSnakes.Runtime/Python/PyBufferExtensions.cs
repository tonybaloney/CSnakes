using CommunityToolkit.HighPerformance;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;
public static class PyBufferExtensions
{
    #region AsSpan
    public static Span<bool> AsBoolSpan(this IPyBuffer buffer) => buffer.AsSpan<bool>();
    public static Span<byte> AsByteSpan(this IPyBuffer buffer) => buffer.AsSpan<byte>();
    public static Span<sbyte> AsSByteSpan(this IPyBuffer buffer) => buffer.AsSpan<sbyte>();

    public static Span<short> AsInt16Span(this IPyBuffer buffer) => buffer.AsSpan<short>();
    public static Span<ushort> AsUInt16Span(this IPyBuffer buffer) => buffer.AsSpan<ushort>();

    public static Span<int> AsInt32Span(this IPyBuffer buffer) => buffer.AsSpan<int>();

    public static Span<uint> AsUInt32Span(this IPyBuffer buffer) => buffer.AsSpan<uint>();

    public static Span<long> AsInt64Span(this IPyBuffer buffer) => buffer.AsSpan<long>();

    public static Span<ulong> AsUInt64Span(this IPyBuffer buffer) => buffer.AsSpan<ulong>();

    public static Span<float> AsFloatSpan(this IPyBuffer buffer) => buffer.AsSpan<float>();

    public static Span<double> AsDoubleSpan(this IPyBuffer buffer) => buffer.AsSpan<double>();
    public static Span<nint> AsIntPtrSpan(this IPyBuffer buffer) => buffer.AsSpan<nint>();
    public static Span<nuint> AsUIntPtrSpan(this IPyBuffer buffer) => buffer.AsSpan<nuint>();
    #endregion

    #region AsReadOnlySpan
    public static ReadOnlySpan<bool> AsBoolReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<bool>();
    public static ReadOnlySpan<byte> AsByteReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<byte>();
    public static ReadOnlySpan<sbyte> AsSByteReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<sbyte>();
    public static ReadOnlySpan<short> AsInt16ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<short>();
    public static ReadOnlySpan<ushort> AsUInt16ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<ushort>();
    public static ReadOnlySpan<int> AsInt32ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<int>();
    public static ReadOnlySpan<uint> AsUInt32ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<uint>();
    public static ReadOnlySpan<long> AsInt64ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<long>();
    public static ReadOnlySpan<ulong> AsUInt64ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<ulong>();
    public static ReadOnlySpan<float> AsFloatReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<float>();
    public static ReadOnlySpan<double> AsDoubleReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<double>();
    public static ReadOnlySpan<nint> AsIntPtrReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<nint>();
    public static ReadOnlySpan<nuint> AsUIntPtrReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<nuint>();
    #endregion

    #region AsSpan2D
    public static Span2D<bool> AsBoolSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<bool>();
    public static Span2D<byte> AsByteSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<byte>();
    public static Span2D<sbyte> AsSByteSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<sbyte>();
    public static Span2D<short> AsInt16Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<short>();
    public static Span2D<ushort> AsUInt16Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<ushort>();
    public static Span2D<int> AsInt32Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<int>();
    public static Span2D<uint> AsUInt32Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<uint>();
    public static Span2D<long> AsInt64Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<long>();
    public static Span2D<ulong> AsUInt64Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<ulong>();
    public static Span2D<float> AsFloatSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<float>();
    public static Span2D<double> AsDoubleSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<double>();
    public static Span2D<nint> AsIntPtrSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<nint>();
    public static Span2D<nuint> AsUIntPtrSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<nuint>();
    #endregion

    #region AsReadOnlySpan2D
    public static ReadOnlySpan2D<bool> AsBoolReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D< bool>();
    public static ReadOnlySpan2D<byte> AsByteReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<byte>();
    public static ReadOnlySpan2D<sbyte> AsSByteReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<sbyte>();
    public static ReadOnlySpan2D<short> AsInt16ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<short>();
    public static ReadOnlySpan2D<ushort> AsUInt16ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<ushort>();
    public static ReadOnlySpan2D<int> AsInt32ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<int>();
    public static ReadOnlySpan2D<uint> AsUInt32ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<uint>();
    public static ReadOnlySpan2D<long> AsInt64ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<long>();
    public static ReadOnlySpan2D<ulong> AsUInt64ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<ulong>();
    public static ReadOnlySpan2D<float> AsFloatReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<float>();
    public static ReadOnlySpan2D<double> AsDoubleReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<double>();
    public static ReadOnlySpan2D<nint> AsIntPtrReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<nint>();
    public static ReadOnlySpan2D<nuint> AsUIntPtrReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<nuint>();
    #endregion

    #region AsTensorSpan
#if NET9_0_OR_GREATER
    public static TensorSpan<bool> AsBoolTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<bool>();
    public static TensorSpan<byte> AsByteTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<byte>();
    public static TensorSpan<sbyte> AsSByteTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<sbyte>();
    public static TensorSpan<short> AsInt16TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<short>();
    public static TensorSpan<ushort> AsUInt16TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<ushort>();
    public static TensorSpan<int> AsInt32TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<int>();
    public static TensorSpan<uint> AsUInt32TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<uint>();
    public static TensorSpan<long> AsInt64TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<long>();
    public static TensorSpan<ulong> AsUInt64TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<ulong>();
    public static TensorSpan<float> AsFloatTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<float>();
    public static TensorSpan<double> AsDoubleTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<double>();
    public static TensorSpan<nint> AsIntPtrTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<nint>();
    public static TensorSpan<nuint> AsUIntPtrTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<nuint>();

    public static ReadOnlyTensorSpan<bool> AsBoolReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<bool>();
    public static ReadOnlyTensorSpan<byte> AsByteReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<byte>();
    public static ReadOnlyTensorSpan<sbyte> AsSByteReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<sbyte>();
    public static ReadOnlyTensorSpan<short> AsInt16ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<short>();
    public static ReadOnlyTensorSpan<ushort> AsUInt16ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<ushort>();
    public static ReadOnlyTensorSpan<int> AsInt32ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<int>();
    public static ReadOnlyTensorSpan<uint> AsUInt32ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<uint>();
    public static ReadOnlyTensorSpan<long> AsInt64ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<long>();
    public static ReadOnlyTensorSpan<ulong> AsUInt64ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<ulong>();
    public static ReadOnlyTensorSpan<float> AsFloatReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<float>();
    public static ReadOnlyTensorSpan<double> AsDoubleReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<double>();
    public static ReadOnlyTensorSpan<nint> AsIntPtrReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<nint>();
    public static ReadOnlyTensorSpan<nuint> AsUIntPtrReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<nuint>();
#endif
    #endregion
}
