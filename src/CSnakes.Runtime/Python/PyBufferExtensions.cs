using CommunityToolkit.HighPerformance;


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


}
