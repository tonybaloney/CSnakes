using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;

namespace Profile;

public class CallBenchmarks : BaseBenchmark
{
    private ICallBenchmarks mod = null!;

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.CallBenchmarks();
    }

    [Benchmark]
    public void PositionalOnlyArgs()
    {
        mod.PositionalOnlyArgs(1, 2, 3);
    }

    [Benchmark]
    public void CollectStarArgs()
    {
        using var arg = PyObject.From(3);
        mod.CollectStarArgs(1, 2, [arg]);
    }

    [Benchmark]
    public void CollectStarStarKwargs()
    {
        using PyObject arg = PyObject.From(3L);
        mod.CollectStarStarKwargs(1, 2, [new("c", arg)]);
    }

    [Benchmark]
    public void PositionalAndKwargs()
    {
        using PyObject arg = PyObject.From(3L);
        mod.PositionalAndKwargs(a: 1, b: 2, c: 3, kwargs: [new("d", arg)]);
    }

    [Benchmark]
    public void KeywordOnly()
    {
        mod.KeywordOnlyArgs(1, b: 2, c: 3);
    }

    [Benchmark]
    public void CollectStarArgsAndKeywordOnlyArgs()
    {
        using var arg = PyObject.From(3L);
        mod.CollectStarArgsAndKeywordOnlyArgs(a: 1, b: 2, c: 3, args: [arg]);
    }
}
