using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;

namespace Profile;

public class CallBenchmarks : BaseBenchmark
{
    private ICallBenchmarks mod = null!;
    private PyObject kw = null!;

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

    [GlobalSetup(Target = nameof(CollectStarStarKwargs))]
    public void CollectStarStarKwargsSetup()
    {
        Setup();
        kw = PyObject.From("c");
    }

    [Benchmark]
    public void CollectStarStarKwargs()
    {
        using PyObject arg = PyObject.From(3L);
        mod.CollectStarStarKwargs(1, 2, [new(kw, arg)]);
    }

    [GlobalSetup(Target = nameof(PositionalAndKwargs))]
    public void PositionalAndKwargsSetup()
    {
        Setup();
        kw = PyObject.From("d");
    }

    [Benchmark]
    public void PositionalAndKwargs()
    {
        using PyObject arg = PyObject.From(3L);
        mod.PositionalAndKwargs(a: 1, b: 2, c: 3, kwargs: [new(kw, arg)]);
    }

    [GlobalCleanup(Targets = [nameof(CollectStarStarKwargs), nameof(PositionalAndKwargsSetup)])]
    public void Cleanup()
    {
        kw.Dispose();
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

    [Benchmark]
    public void ManyKeywordOnly()
    {
        mod.ManyKeywordOnlyArgs(PyObject.None, []);
    }
}
