using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManagedPython.Tests;
public class ManagedPythonTestBase : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public ManagedPythonTestBase()
    {
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromManagedPython()
                    .WithPipInstaller()
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
