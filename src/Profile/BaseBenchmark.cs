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
        
        string home = Path.Join(Environment.CurrentDirectory, "python");
        builder.Services.WithPython()
            .WithHome(home)
            .WithVirtualEnvironment(Path.Join(home, ".venv"))
            .WithUvInstaller()
            .FromRedistributable();
        
        IHost app = builder.Build();

        Env = app.Services.GetRequiredService<IPythonEnvironment>();
    }
}
