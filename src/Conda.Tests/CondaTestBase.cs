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
        string condaEnv = Environment.GetEnvironmentVariable("CONDA") ?? string.Empty;
        string? pythonVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION");

        if (string.IsNullOrEmpty(condaEnv))
        {
            string? basePath = null;
            if (OperatingSystem.IsWindows())
            {
                basePath = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            }
            else
            {
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            if (basePath is not null)
            {
                condaEnv = new[] { "anaconda3", "miniconda3" }
                    .Select(condaName => Path.Join(basePath, condaName))
                    .FirstOrDefault(Directory.Exists) ?? condaEnv;
            }

        }

        var condaBinPath = OperatingSystem.IsWindows() ? Path.Join(condaEnv, "Scripts", "conda.exe") : Path.Join(condaEnv, "bin", "conda");
        var environmentSpecPath = Path.Join(Environment.CurrentDirectory, "python", "environment.yml");
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromConda(condaBinPath)
                  .WithCondaEnvironment("csnakes_test", environmentSpecPath, true, pythonVersion);

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
