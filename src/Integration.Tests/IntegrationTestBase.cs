using CSnakes.Runtime.PackageManagement;
using CSnakes.Runtime.Tests;
using Microsoft.Extensions.DependencyInjection;
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
public sealed class PythonEnvironmentFixture : RedistributablePythonEnvironmentFixture
{
    public PythonEnvironmentFixture(RedistributablePythonSkuFixture skuFixture) :
        base(skuFixture, home: Path.Join(Environment.CurrentDirectory, "python"),
             context => context.Environment
                               .WithVirtualEnvironment(skuFixture.VirtualEnvironmentPath)
                               .WithPipInstaller())
    {
        Installer = Services.GetRequiredService<IPythonPackageInstaller>();
        VirtualEnvironmentPath = skuFixture.VirtualEnvironmentPath;
    }

    /// <summary>The Python home directory shared by all tests in the collection.</summary>
    public string Home { get; } = Path.Join(Environment.CurrentDirectory, "python");

    public string VirtualEnvironmentPath { get; }

    public IPythonPackageInstaller Installer { get; }
}

[Collection(PythonEnvironmentCollection.Name)]
public abstract class IntegrationTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
    public IPythonPackageInstaller Installer { get; } = fixture.Installer;
}
