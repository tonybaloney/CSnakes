using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CSnakes.Runtime.Tests;


public class TestEnvironment : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public TestEnvironment()
    {
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Environment.CurrentDirectory);

                pb.FromNuGet("3.12.4").FromMacOSInstallerLocator("3.12").FromEnvironmentVariable("Python3_ROOT_DIR", "3.12.4");
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            app.Dispose();
        }
    }

    public IPythonEnvironment Env => env;
}
