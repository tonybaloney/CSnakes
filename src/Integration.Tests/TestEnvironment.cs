using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;

namespace Integration.Tests;
public class TestEnvironment : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public TestEnvironment()
    {
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.WithPython(Path.Join(Environment.CurrentDirectory, "python"));

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    services.WithPythonFromNuGet("3.12.4");
                }
                else
                {
                    services.WithPythonFromEnvironmentVariable("Python3_ROOT_DIR", "3.12.4");
                }
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        app.Dispose();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            env.Dispose();
        }
    }

    public IPythonEnvironment Env => env;
}
