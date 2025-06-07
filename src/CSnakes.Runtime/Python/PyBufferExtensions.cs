using CommunityToolkit.HighPerformance;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;

#pragma warning disable CS0612 // Member is obsolete

[Obsolete]
public static class PyBufferExtensions
{
    #region AsSpan
    [Obsolete] public static Span<bool> AsBoolSpan(this IPyBuffer buffer) => buffer.AsSpan<bool>();
    [Obsolete] public static Span<byte> AsByteSpan(this IPyBuffer buffer) => buffer.AsSpan<byte>();
    [Obsolete] public static Span<sbyte> AsSByteSpan(this IPyBuffer buffer) => buffer.AsSpan<sbyte>();

    [Obsolete] public static Span<short> AsInt16Span(this IPyBuffer buffer) => buffer.AsSpan<short>();
    [Obsolete] public static Span<ushort> AsUInt16Span(this IPyBuffer buffer) => buffer.AsSpan<ushort>();

    [Obsolete] public static Span<int> AsInt32Span(this IPyBuffer buffer) => buffer.AsSpan<int>();

    [Obsolete] public static Span<uint> AsUInt32Span(this IPyBuffer buffer) => buffer.AsSpan<uint>();

    [Obsolete] public static Span<long> AsInt64Span(this IPyBuffer buffer) => buffer.AsSpan<long>();

    [Obsolete] public static Span<ulong> AsUInt64Span(this IPyBuffer buffer) => buffer.AsSpan<ulong>();

    [Obsolete] public static Span<float> AsFloatSpan(this IPyBuffer buffer) => buffer.AsSpan<float>();

    [Obsolete] public static Span<double> AsDoubleSpan(this IPyBuffer buffer) => buffer.AsSpan<double>();
    [Obsolete] public static Span<nint> AsIntPtrSpan(this IPyBuffer buffer) => buffer.AsSpan<nint>();
    [Obsolete] public static Span<nuint> AsUIntPtrSpan(this IPyBuffer buffer) => buffer.AsSpan<nuint>();
    #endregion

    #region AsReadOnlySpan
    [Obsolete] public static ReadOnlySpan<bool> AsBoolReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<bool>();
    [Obsolete] public static ReadOnlySpan<byte> AsByteReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<byte>();
    [Obsolete] public static ReadOnlySpan<sbyte> AsSByteReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<sbyte>();
    [Obsolete] public static ReadOnlySpan<short> AsInt16ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<short>();
    [Obsolete] public static ReadOnlySpan<ushort> AsUInt16ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<ushort>();
    [Obsolete] public static ReadOnlySpan<int> AsInt32ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<int>();
    [Obsolete] public static ReadOnlySpan<uint> AsUInt32ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<uint>();
    [Obsolete] public static ReadOnlySpan<long> AsInt64ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<long>();
    [Obsolete] public static ReadOnlySpan<ulong> AsUInt64ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<ulong>();
    [Obsolete] public static ReadOnlySpan<float> AsFloatReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<float>();
    [Obsolete] public static ReadOnlySpan<double> AsDoubleReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<double>();
    [Obsolete] public static ReadOnlySpan<nint> AsIntPtrReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<nint>();
    [Obsolete] public static ReadOnlySpan<nuint> AsUIntPtrReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<nuint>();
    #endregion

    #region AsSpan2D
    [Obsolete] public static Span2D<bool> AsBoolSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<bool>();
    [Obsolete] public static Span2D<byte> AsByteSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<byte>();
    [Obsolete] public static Span2D<sbyte> AsSByteSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<sbyte>();
    [Obsolete] public static Span2D<short> AsInt16Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<short>();
    [Obsolete] public static Span2D<ushort> AsUInt16Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<ushort>();
    [Obsolete] public static Span2D<int> AsInt32Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<int>();
    [Obsolete] public static Span2D<uint> AsUInt32Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<uint>();
    [Obsolete] public static Span2D<long> AsInt64Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<long>();
    [Obsolete] public static Span2D<ulong> AsUInt64Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<ulong>();
    [Obsolete] public static Span2D<float> AsFloatSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<float>();
    [Obsolete] public static Span2D<double> AsDoubleSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<double>();
    [Obsolete] public static Span2D<nint> AsIntPtrSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<nint>();
    [Obsolete] public static Span2D<nuint> AsUIntPtrSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<nuint>();
    #endregion

    #region AsReadOnlySpan2D
    [Obsolete] public static ReadOnlySpan2D<bool> AsBoolReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<bool>();
    [Obsolete] public static ReadOnlySpan2D<byte> AsByteReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<byte>();
    [Obsolete] public static ReadOnlySpan2D<sbyte> AsSByteReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<sbyte>();
    [Obsolete] public static ReadOnlySpan2D<short> AsInt16ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<short>();
    [Obsolete] public static ReadOnlySpan2D<ushort> AsUInt16ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<ushort>();
    [Obsolete] public static ReadOnlySpan2D<int> AsInt32ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<int>();
    [Obsolete] public static ReadOnlySpan2D<uint> AsUInt32ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<uint>();
    [Obsolete] public static ReadOnlySpan2D<long> AsInt64ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<long>();
    [Obsolete] public static ReadOnlySpan2D<ulong> AsUInt64ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<ulong>();
    [Obsolete] public static ReadOnlySpan2D<float> AsFloatReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<float>();
    [Obsolete] public static ReadOnlySpan2D<double> AsDoubleReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<double>();
    [Obsolete] public static ReadOnlySpan2D<nint> AsIntPtrReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<nint>();
    [Obsolete] public static ReadOnlySpan2D<nuint> AsUIntPtrReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<nuint>();
    #endregion

    #region AsTensorSpan
#if NET9_0_OR_GREATER
    [Obsolete] public static TensorSpan<bool> AsBoolTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<bool>();
    [Obsolete] public static TensorSpan<byte> AsByteTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<byte>();
    [Obsolete] public static TensorSpan<sbyte> AsSByteTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<sbyte>();
    [Obsolete] public static TensorSpan<short> AsInt16TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<short>();
    [Obsolete] public static TensorSpan<ushort> AsUInt16TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<ushort>();
    [Obsolete] public static TensorSpan<int> AsInt32TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<int>();
    [Obsolete] public static TensorSpan<uint> AsUInt32TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<uint>();
    [Obsolete] public static TensorSpan<long> AsInt64TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<long>();
    [Obsolete] public static TensorSpan<ulong> AsUInt64TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<ulong>();
    [Obsolete] public static TensorSpan<float> AsFloatTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<float>();
    [Obsolete] public static TensorSpan<double> AsDoubleTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<double>();
    [Obsolete] public static TensorSpan<nint> AsIntPtrTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<nint>();
    [Obsolete] public static TensorSpan<nuint> AsUIntPtrTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<nuint>();

    [Obsolete] public static ReadOnlyTensorSpan<bool> AsBoolReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<bool>();
    [Obsolete] public static ReadOnlyTensorSpan<byte> AsByteReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<byte>();
    [Obsolete] public static ReadOnlyTensorSpan<sbyte> AsSByteReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<sbyte>();
    [Obsolete] public static ReadOnlyTensorSpan<short> AsInt16ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<short>();
    [Obsolete] public static ReadOnlyTensorSpan<ushort> AsUInt16ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<ushort>();
    [Obsolete] public static ReadOnlyTensorSpan<int> AsInt32ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<int>();
    [Obsolete] public static ReadOnlyTensorSpan<uint> AsUInt32ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<uint>();
    [Obsolete] public static ReadOnlyTensorSpan<long> AsInt64ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<long>();
    [Obsolete] public static ReadOnlyTensorSpan<ulong> AsUInt64ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<ulong>();
    [Obsolete] public static ReadOnlyTensorSpan<float> AsFloatReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<float>();
    [Obsolete] public static ReadOnlyTensorSpan<double> AsDoubleReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<double>();
    [Obsolete] public static ReadOnlyTensorSpan<nint> AsIntPtrReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<nint>();
    [Obsolete] public static ReadOnlyTensorSpan<nuint> AsUIntPtrReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<nuint>();
#endif
    #endregion
}
