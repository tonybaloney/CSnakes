using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime.Tests;

public class RuntimeTestBase : IDisposable
{
    protected readonly IPythonEnvironment env;
    protected readonly IHost app;

    public RuntimeTestBase()
    {
        string pythonVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";
        string shortVersion = string.Join('.', pythonVersion.Split('.').Take(2));

        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Environment.CurrentDirectory);

                pb
                  .FromEnvironmentVariable("", pythonVersion)  // for GitHub Actions
                  .FromRedistributable(version: shortVersion, freeThreaded: freeThreaded);

                services.AddLogging(builder => builder.AddXUnit());
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }
}
