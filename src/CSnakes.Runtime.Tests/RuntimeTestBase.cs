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
        string pythonVersionWindows = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9";
        string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";

        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython();
        pb.WithHome(Environment.CurrentDirectory);

        pb
          .FromNuGet(pythonVersionWindows)
          .FromMacOSInstallerLocator(pythonVersionMacOS, freeThreaded)
          .FromWindowsStore("3.12")
          .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersionLinux); // This last one is for GitHub Actions

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
