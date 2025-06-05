using System;
using System.IO;
using System.Linq;
using CSnakes.Runtime.PackageManagement;
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
    private readonly IPythonPackageInstaller installer;

    private readonly IHost app;

    public PythonEnvironmentFixture()
    {
        string pythonVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", ".venv");
        string shortVersion = string.Join('.', pythonVersion.Split('.').Take(2));

        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython()
          .WithHome(Path.Join(Environment.CurrentDirectory, "python"))
          .FromRedistributable(shortVersion, freeThreaded)
          .WithVirtualEnvironment(venvPath)
          .WithPipInstaller();

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddXUnit());
        
        app = builder.Build();

        env = app.Services.GetRequiredService<IPythonEnvironment>();
        installer = app.Services.GetRequiredService<IPythonPackageInstaller>();
    }

    public void Dispose()
    {
        env.Dispose();
        app.Dispose();
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    public IPythonEnvironment Env => env;
    public IPythonPackageInstaller Installer => installer;
}

[Collection(PythonEnvironmentCollection.Name)]
public abstract class IntegrationTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
    public IPythonPackageInstaller Installer { get; } = fixture.Installer;
}
