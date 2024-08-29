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
    public void FunctionReturnsList()
    {
        mod!.GenerateSequence();
    }

    [Benchmark]
    public void FunctionTakesList()
    {
        List<long> hundredNumbers = [];
        for (int i = 0; i < 100; i++)
        {
            hundredNumbers.Add(i);
        }
        mod!.ConsumeSequence(hundredNumbers);
    }

    [Benchmark]
    public void FunctionTakesDictionary()
    {
        Dictionary<string, long> hundredNumbers = [];
        for (int i = 0; i < 100; i++)
        {
            hundredNumbers.Add(i.ToString(), i);
        }
        mod!.ConsumeDictionary(hundredNumbers);
    }

    [Benchmark]
    public void FunctionReturnsDictionary()
    {
        mod!.GenerateDictionary();
    }

    [Benchmark]
    public void FunctionReturnsTuple()
    {
        mod!.GenerateTuple();
    }

    [Benchmark]
    public void FunctionTakesTuple()
    {
        mod!.ConsumeTuple((5, "hello", 4.2, true));
    }

    [Benchmark]
    public void FunctionTakesValueTypes()
    {
        mod!.ConsumeValueTypes(5, "hello", 4.2, true);
    }

    [Benchmark]
    public void EmptyFunction()
    {
        mod!.EmptyFunction();
    }
}
