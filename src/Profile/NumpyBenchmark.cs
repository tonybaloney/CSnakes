using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using System.Numerics.Tensors;
using System.Numerics;

namespace Profile;


[RPlotExporter]
public class NumpyBenchmark : BaseBenchmark
{
    private INumpyBenchmarks? mod;

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.NumpyBenchmarks();
    }

    [Benchmark]
    public void Float32Validate()
    {
        using var buffer = mod!.Generate2dFloat32Array(100, 100);
        ReadOnlyTensorSpan<float> resultTensor = buffer.AsReadOnlyTensorSpan<float>();
        // Calculate exponent
        var exp = Tensor.Multiply(resultTensor, resultTensor);
    }

    [Benchmark]
    public void Float64Validate()
    {
        using var buffer = mod!.Generate2dFloat64Array(100, 100);
        ReadOnlyTensorSpan<double> resultTensor = buffer.AsReadOnlyTensorSpan<double>();
        // Calculate exponent
        var exp = Tensor.Multiply(resultTensor, resultTensor);
    }

    [Benchmark]
    public void BFloat16Validate()
    {
        using var buffer = mod!.Generate2dBfloat16Array(100, 100);
        ReadOnlyTensorSpan<BFloat16> resultTensor = buffer.AsReadOnlyTensorSpan<BFloat16>();
        // Calculate exponent
        var exp = Tensor.Multiply(resultTensor, resultTensor);
    }
}