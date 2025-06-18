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
        string pythonVersionWindows = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9";
        string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";


        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython();
        pb.WithHome(Path.Join(Environment.CurrentDirectory));

        pb.FromRedistributable();
        
        IHost app = builder.Build();

        Env = app.Services.GetRequiredService<IPythonEnvironment>();
    }
}
