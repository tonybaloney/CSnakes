using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Profile;

[MemoryDiagnoser]
public class BaseBenchmark
{
    protected readonly IPythonEnvironment Env;

    public BaseBenchmark()
    {
        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython();
        pb.WithHome(Path.Join(Environment.CurrentDirectory))
          .FromRedistributable(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12");

        IHost app = builder.Build();

        Env = app.Services.GetRequiredService<IPythonEnvironment>();
    }
}
