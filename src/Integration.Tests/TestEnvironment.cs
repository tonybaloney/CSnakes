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
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromNuGet("3.12.4").FromEnvironmentVariable("Python3_ROOT_DIR", "3.12.4");
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
