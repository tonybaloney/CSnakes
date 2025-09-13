using CSnakes;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using CSnakes.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

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

    public PythonEnvironmentFixture()
    {
        Version pythonVersionToTest = ServiceCollectionExtensions.ParsePythonVersion(Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.9");
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";
        bool debugPython = Environment.GetEnvironmentVariable("PYTHON_DEBUG") == "1";
        string venvPath = Path.Join(Environment.CurrentDirectory, "python", $".venv-{pythonVersionToTest}{(freeThreaded ? "t": "")}{(debugPython ? "d": "")}");

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

        var config = PythonEnvironmentConfiguration.Default
                            .SetHome(Path.Join(Environment.CurrentDirectory, "python"))
                            .FromRedistributable(version: redistributableVersion, debug: debugPython, freeThreaded: freeThreaded)
                            .SetVirtualEnvironment(venvPath)
                            .WithXUnitLogging(null)
                            .AddPipInstaller();

        env = config.GetPythonEnvironment();
        installer = config.PackageInstallers.Single();
    }

    public void Dispose()
    {
        env.Dispose();
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
