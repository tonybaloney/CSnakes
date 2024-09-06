using CommunityToolkit.HighPerformance;

namespace CSnakes.Runtime.Python;
public interface IPyBuffer
{
    Int64 Length { get; }
    int Dimensions { get; }

    bool Scalar { get; }

    Span<int> AsInt32Span();
    Span<long> AsInt64Span();
    Span<uint> AsUInt32Span();
    Span<ulong> AsUInt64Span();
    Span<float> AsFloatSpan();
    Span<double> AsDoubleSpan();

    Span2D<int> AsInt32Span2D();
    Span2D<uint> AsUInt32Span2D();
    Span2D<long> AsInt64Span2D();
    Span2D<ulong> AsUInt64Span2D();
    Span2D<float> AsFloatSpan2D();
    Span2D<double> AsDoubleSpan2D();
}
