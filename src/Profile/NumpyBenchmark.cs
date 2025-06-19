using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using System.Numerics.Tensors;
using System.Numerics;
using CSnakes.Runtime.Python;

namespace Profile;


[MemoryDiagnoser]
[RPlotExporter]
public class NumpyBenchmark : BaseBenchmark
{
    private INumpyBenchmarks? mod;

    private static unsafe ReadOnlyTensorSpan<T> AsTensor<T>(IPyBuffer buffer, nint[] dimensions) where T : unmanaged
    {
        var strides = new nint[] { dimensions[0], 1 };

        return new ReadOnlyTensorSpan<T>(
            (T*)buffer.GetAddressOf(),
            dimensions[0] * dimensions[1] * sizeof(T), // Length
            dimensions, // Dimensions
            strides // Strides
        );
    }

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.NumpyBenchmarks();
    }

    [Benchmark]
    public void Float32CreateTensor()
    {
        using var buffer = mod!.Generate2dFloat32Array(100, 100);
        ReadOnlyTensorSpan<float> resultTensor = buffer.AsReadOnlyTensorSpan<float>();
    }

    [Benchmark]
    public void Float32TensorMultiply()
    {
        var dimensions = new nint[] { 100, 100 };
        using var buffer = mod!.Generate2dFloat32ArrayAsBytes(dimensions[0], dimensions[1]);
        var tensor = AsTensor<float>(buffer, dimensions);
        // Calculate exponent
        var exp = Tensor.Multiply(tensor, tensor);
    }

    [Benchmark]
    public void Float32CreateTensorFromBytes()
    {
        var dimensions = new nint[] { 100, 100 };
        using var buffer = mod!.Generate2dFloat32ArrayAsBytes(dimensions[0], dimensions[1]);
        var tensor = AsTensor<float>(buffer, dimensions);
    }

    [Benchmark]
    public void Float64CreateTensor()
    {
        using var buffer = mod!.Generate2dFloat64Array(100, 100);
        ReadOnlyTensorSpan<double> resultTensor = buffer.AsReadOnlyTensorSpan<double>();
    }

    [Benchmark]
    public void Float64CreateTensorFromBytes()
    {
        var dimensions = new nint[] { 100, 100 };
        using var buffer = mod!.Generate2dFloat64ArrayAsBytes(dimensions[0], dimensions[1]);
        var tensor  = AsTensor<double>(buffer, dimensions);
    }

    [Benchmark]
    public void Float64TensorMultiply()
    {
        var dimensions = new nint[] { 100, 100 };
        using var buffer = mod!.Generate2dFloat64ArrayAsBytes(dimensions[0], dimensions[1]);
        var tensor = AsTensor<double>(buffer, dimensions);
        // Calculate exponent
        var exp = Tensor.Multiply(tensor, tensor);
    }

    // Not possible right now.
    //[Benchmark]
    //public void BFloat16CreateTensor()
    //{
    //    using var buffer = mod!.Generate2dBfloat16Array(100, 100);
    //    ReadOnlyTensorSpan<BFloat16> resultTensor = buffer.AsReadOnlyTensorSpan<BFloat16>();
    //    // Calculate exponent
    //    var exp = Tensor.Multiply(resultTensor, resultTensor);
    //}

    [Benchmark]
    public void BFloat16CreateTensorFromBytes()
    {
        var dimensions = new nint[] { 100, 100 };
        using var buffer = mod!.Generate2dBfloat16ArrayAsBytes(dimensions[0], dimensions[1]);
        var tensor = AsTensor<BFloat16>(buffer, dimensions);
    }

    [Benchmark]
    public void BFloat16TensorMultiply()
    {
        var dimensions = new nint[] { 100, 100 };
        using var buffer = mod!.Generate2dBfloat16ArrayAsBytes(dimensions[0], dimensions[1]);
        var tensor = AsTensor<BFloat16>(buffer, dimensions);
        // Calculate exponent
        var exp = Tensor.Multiply(tensor, tensor);
    }
}
