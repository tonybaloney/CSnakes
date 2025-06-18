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

    [Params(1, 10, 100, 1_000)]
    public int N { get; set; }

    [Params(0.001, 1)]
    public double Delay { get; set; }

    [Benchmark]
    public async Task AsyncFunction()
    {
        if (N == 1)
        {
            await mod!.AsyncSleepy(Delay);
        }
        else
        {
            var tasks =
                from n in Enumerable.Range(0, N)
                select mod!.AsyncSleepy(Delay);

            await Task.WhenAll(tasks);
        }
    }
}
