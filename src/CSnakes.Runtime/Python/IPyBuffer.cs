namespace CSnakes.Runtime.Python;
public interface IPyBuffer
{
    Int64 Length { get; }

    bool Scalar { get; }

    Span<Int32> AsInt32Scalar();
    Span<Int64> AsInt64Scalar();
    Span<UInt32> AsUInt32Scalar();
    Span<UInt64> AsUInt64Scalar();
    public Span<float> AsFloatScalar();
}
