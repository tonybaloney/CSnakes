using CSnakes.Runtime;
using BenchmarkDotNet.Attributes;


namespace Profile;

[MarkdownExporter]
public class MarshallingBenchmarks: BaseBenchmark
{
    private IMarshallingBenchmarks? mod ;

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.MarshallingBenchmarks();
    }

    [Benchmark]
    public void ComplexReturn()
    {
        mod!.GenerateData(5, "hello", (3.2, "testinput"), false);
    }

    [Benchmark]
    public void ComplexReturnLazy()
    {
        mod!.GenerateDataAny(5, "hello", (3.2, "testinput"), false);
    }

    [Benchmark]
    public void EmptyFunction()
    {
        mod!.EmptyFunction();
    }
}
