using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

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
        Version pythonVersionToTest = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "1";
        bool debugPython = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", $".venv-{pythonVersionToTest}");

        RedistributablePythonVersion redistributableVersion = pythonVersionToTest.Minor switch
        {
            9 => RedistributablePythonVersion.Python3_9,
            10 => RedistributablePythonVersion.Python3_10,
            11 => RedistributablePythonVersion.Python3_11,
            12 => RedistributablePythonVersion.Python3_12,
            13 => RedistributablePythonVersion.Python3_13,
            14 => RedistributablePythonVersion.Python3_14,
            _ => throw new NotSupportedException($"Python version {pythonVersionToTest} is not supported.")
        };

        var builder = Host.CreateApplicationBuilder();
        var pb = builder.Services.WithPython();
        pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

        pb.FromRedistributable(version: redistributableVersion, debug: debugPython, freeThreaded: freeThreaded)
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
