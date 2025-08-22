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

        var builder = Host.CreateApplicationBuilder();
        builder.Services
          .WithPython()
          .WithHome(Environment.CurrentDirectory)
          .FromRedistributable(version: shortVersion, freeThreaded: freeThreaded);

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddXUnit());
        
        app = builder.Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }
}
