using BenchmarkDotNet.Attributes;
using CSnakes;
using CSnakes.Runtime;

namespace Profile;

[MemoryDiagnoser]
public class BaseBenchmark
{
    protected readonly IPythonEnvironment Env;

    public BaseBenchmark()
    {
        Env = Python.GetEnvironment(pb => pb.WithHome(Path.Join(Environment.CurrentDirectory))
                                            .FromRedistributable(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12"));
    }
}
