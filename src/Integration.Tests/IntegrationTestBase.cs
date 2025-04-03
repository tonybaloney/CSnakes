using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Integration.Tests;

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
[CollectionDefinition(Name)]
public sealed class PythonEnvironmentCollection : ICollectionFixture<PythonEnvironmentFixture>
{
    public const string Name = "PythonEnvironment";
}

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
public sealed class PythonEnvironmentFixture : IDisposable
{
    private readonly IPythonEnvironment env;
    private readonly IHost app;

    public PythonEnvironmentFixture()
    {
        string pythonVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");
        string shortVersion = string.Join('.', pythonVersion.Split('.').Take(2));

        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb
                  .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersion)  // for GitHub Actions
                  .FromRedistributable(shortVersion, freeThreaded)
                  .WithVirtualEnvironment(venvPath)
                  .WithPipInstaller();

                services.AddLogging(builder => builder.AddXUnit());
            })
            .Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
    }

    public void Dispose()
    {
        env.Dispose();
        app.Dispose();
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    public IPythonEnvironment Env => env;
}

[Collection(PythonEnvironmentCollection.Name)]
public abstract class IntegrationTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
}
