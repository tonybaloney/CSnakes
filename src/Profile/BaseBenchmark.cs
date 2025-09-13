using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;

namespace Profile;

[MemoryDiagnoser]
public class BaseBenchmark
{
    protected readonly IPythonEnvironment Env;

    public BaseBenchmark()
    {
        Env = PythonEnvironmentConfiguration.Default.SetHome(Environment.CurrentDirectory)
                                            .FromRedistributable(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12")
                                            .GetPythonEnvironment();
    }
}
