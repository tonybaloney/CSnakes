using CommunityToolkit.HighPerformance;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;

[Obsolete($"Cast an '{nameof(IPyBuffer)}' to a subclass of '{nameof(PyBuffer<>)}' with the appropriate type argument instead.")]
public static class PyBufferExtensions
{
    #region AsSpan
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<bool>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<bool> AsBoolSpan(this IPyBuffer buffer) => buffer.AsSpan<bool>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<byte>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<byte> AsByteSpan(this IPyBuffer buffer) => buffer.AsSpan<byte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<sbyte>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<sbyte> AsSByteSpan(this IPyBuffer buffer) => buffer.AsSpan<sbyte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<short>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<short> AsInt16Span(this IPyBuffer buffer) => buffer.AsSpan<short>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ushort>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<ushort> AsUInt16Span(this IPyBuffer buffer) => buffer.AsSpan<ushort>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<int>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<int> AsInt32Span(this IPyBuffer buffer) => buffer.AsSpan<int>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<uint>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<uint> AsUInt32Span(this IPyBuffer buffer) => buffer.AsSpan<uint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<long>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<long> AsInt64Span(this IPyBuffer buffer) => buffer.AsSpan<long>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ulong>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<ulong> AsUInt64Span(this IPyBuffer buffer) => buffer.AsSpan<ulong>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<Half>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<Half> AsHalfSpan(this IPyBuffer buffer) => buffer.AsSpan<Half>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<float>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<float> AsFloatSpan(this IPyBuffer buffer) => buffer.AsSpan<float>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<double>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<double> AsDoubleSpan(this IPyBuffer buffer) => buffer.AsSpan<double>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nint>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<nint> AsIntPtrSpan(this IPyBuffer buffer) => buffer.AsSpan<nint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nuint>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span<nuint> AsUIntPtrSpan(this IPyBuffer buffer) => buffer.AsSpan<nuint>();
    #endregion

    #region AsReadOnlySpan
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<bool>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<bool> AsBoolReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<bool>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<byte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<byte> AsByteReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<byte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<sbyte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<sbyte> AsSByteReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<sbyte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<short>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<short> AsInt16ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<short>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ushort>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<ushort> AsUInt16ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<ushort>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<int>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<int> AsInt32ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<int>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<uint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<uint> AsUInt32ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<uint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<long>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<long> AsInt64ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<long>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ulong>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<ulong> AsUInt64ReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<ulong>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<Half>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<Half> AsHalfReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<Half>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<float>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<float> AsFloatReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<float>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<double>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<double> AsDoubleReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<double>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<nint> AsIntPtrReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<nint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nuint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan<nuint> AsUIntPtrReadOnlySpan(this IPyBuffer buffer) => buffer.AsReadOnlySpan<nuint>();
    #endregion

    #region AsSpan2D
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<bool>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<bool> AsBoolSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<bool>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<byte>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<byte> AsByteSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<byte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<sbyte>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<sbyte> AsSByteSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<sbyte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<short>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<short> AsInt16Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<short>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ushort>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<ushort> AsUInt16Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<ushort>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<int>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<int> AsInt32Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<int>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<uint>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<uint> AsUInt32Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<uint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<long>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<long> AsInt64Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<long>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ulong>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<ulong> AsUInt64Span2D(this IPyBuffer buffer) => buffer.AsSpan2D<ulong>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<Half>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<Half> AsHalfSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<Half>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<float>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<float> AsFloatSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<float>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<double>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<double> AsDoubleSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<double>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nint>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<nint> AsIntPtrSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<nint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nuint>' and use its '{nameof(PyArrayBuffer<>.Do)}' method instead.")]
    public static Span2D<nuint> AsUIntPtrSpan2D(this IPyBuffer buffer) => buffer.AsSpan2D<nuint>();
    #endregion

    #region AsReadOnlySpan2D
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<bool>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<bool> AsBoolReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<bool>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<byte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<byte> AsByteReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<byte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<sbyte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<sbyte> AsSByteReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<sbyte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<short>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<short> AsInt16ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<short>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ushort>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<ushort> AsUInt16ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<ushort>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<int>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<int> AsInt32ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<int>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<uint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<uint> AsUInt32ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<uint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<long>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<long> AsInt64ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<long>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ulong>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<ulong> AsUInt64ReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<ulong>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<Half>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<Half> AsHalfReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<Half>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<float>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<float> AsFloatReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<float>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<double>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<double> AsDoubleReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<double>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<nint> AsIntPtrReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<nint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nuint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlySpan2D<nuint> AsUIntPtrReadOnlySpan2D(this IPyBuffer buffer) => buffer.AsReadOnlySpan2D<nuint>();
    #endregion

#if NET9_0_OR_GREATER

    #region AsTensorSpan
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<bool>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<bool> AsBoolTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<bool>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<byte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<byte> AsByteTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<byte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<sbyte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<sbyte> AsSByteTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<sbyte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<short>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<short> AsInt16TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<short>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ushort>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<ushort> AsUInt16TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<ushort>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<int>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<int> AsInt32TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<int>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<uint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<uint> AsUInt32TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<uint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<long>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<long> AsInt64TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<long>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ulong>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<ulong> AsUInt64TensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<ulong>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<Half>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<Half> AsHalfTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<Half>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<float>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<float> AsFloatTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<float>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<double>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<double> AsDoubleTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<double>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<nint> AsIntPtrTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<nint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<nuint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static TensorSpan<nuint> AsUIntPtrTensorSpan(this IPyBuffer buffer) => buffer.AsTensorSpan<nuint>();
    #endregion

    #region AsTensorSpan
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<bool>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<bool> AsBoolReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<bool>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<byte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<byte> AsByteReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<byte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<sbyte>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<sbyte> AsSByteReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<sbyte>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<short>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<short> AsInt16ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<short>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<ushort>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<ushort> AsUInt16ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<ushort>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<int>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<int> AsInt32ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<int>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<uint>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<uint> AsUInt32ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<uint>();
    [Obsolete($"Cast to '{nameof(PyArrayBuffer<>)}<long>' and use its '{nameof(PyArrayBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<long> AsInt64ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<long>();
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}<ulong>' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<ulong> AsUInt64ReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<ulong>();
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}<Half>' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<Half> AsHalfReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<Half>();
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}<float>' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<float> AsFloatReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<float>();
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}<double>' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<double> AsDoubleReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<double>();
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}<nint>' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<nint> AsIntPtrReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<nint>();
    [Obsolete($"Cast to '{nameof(PyTensorBuffer<>)}<nuint>' and use its '{nameof(PyTensorBuffer<>.Map)}' method instead.")]
    public static ReadOnlyTensorSpan<nuint> AsUIntPtrReadOnlyTensorSpan(this IPyBuffer buffer) => buffer.AsReadOnlyTensorSpan<nuint>();
    #endregion

#endif // NET9_0_OR_GREATER
}
