using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conda.Tests;
public class CondaTestBase : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public CondaTestBase()
    {
        string pythonVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        string condaEnv = Environment.GetEnvironmentVariable("CONDA") ?? string.Empty;

        if (string.IsNullOrEmpty(condaEnv))
        {
            if (OperatingSystem.IsWindows())
                condaEnv = Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "";
            condaEnv = Path.Join(condaEnv, "anaconda3");

        }
        var condaBinPath = OperatingSystem.IsWindows() ? Path.Join(condaEnv, "Scripts", "conda.exe") : Path.Join("bin", "conda");
        var environmentSpecPath = Path.Join(Environment.CurrentDirectory, "python", "environment.yml");
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromConda(condaBinPath)
                  .WithCondaEnvironment("csnakes_test", environmentSpecPath);

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
