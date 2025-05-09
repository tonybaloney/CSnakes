using System;
using System.IO;
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
    private IPythonEnvironment env;
    private IHost app;

    public PythonEnvironmentFixture()
    {
        CreateEnvironment();
    }

    public void CreateEnvironment()
    {
        string pythonVersionWindows = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9";
        string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");

        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromNuGet(pythonVersionWindows)
                    .FromMacOSInstallerLocator(pythonVersionMacOS, freeThreaded)
                    .FromWindowsStore("3.12")
                    .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersionLinux)
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
