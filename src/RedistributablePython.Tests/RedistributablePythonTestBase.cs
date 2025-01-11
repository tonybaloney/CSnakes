using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RedistributablePython.Tests;
public class RedistributablePythonTestBase : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public RedistributablePythonTestBase()
    {
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromRedistributable()
                  .WithUvInstaller()
                  .WithVirtualEnvironment(venvPath);

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

    public IPythonEnvironment Env => env;
}
