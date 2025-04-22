using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;

namespace Profile;

public class AsyncBenchmarks : BaseBenchmark
{
    private IAsyncBenchmarks? mod;

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.AsyncBenchmarks();
    }

    [Benchmark]
    public async Task AsyncFunction()
    {
        await mod!.AsyncSleepy();
    }
}
