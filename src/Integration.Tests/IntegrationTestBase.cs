using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Integration.Tests;
public class IntegrationTestBase : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public IntegrationTestBase()
    {
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromNuGet("3.12.4").FromMacOSInstallerLocator("3.12").FromEnvironmentVariable("Python3_ROOT_DIR", "3.12.4");

                services.AddLogging(builder => builder.AddXUnit());
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        GC.Collect();
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
