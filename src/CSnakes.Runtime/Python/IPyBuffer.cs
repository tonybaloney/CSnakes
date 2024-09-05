namespace CSnakes.Runtime.Python;
public interface IPyBuffer
{
    Int64 Length { get; }
    int Dimensions { get; }

    bool Scalar { get; }

    Span<int> AsInt32Scalar();
    Span<long> AsInt64Scalar();
    Span<uint> AsUInt32Scalar();
    Span<ulong> AsUInt64Scalar();
    Span<float> AsFloatScalar();
    Span<double> AsDoubleScalar();
}
