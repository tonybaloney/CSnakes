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
    public PythonEnvironmentFixture() :
        base(home: Path.Join(Environment.CurrentDirectory, "python"),
             (context, sku) =>
             {
                 var venv = FormattableString.Invariant($".venv-{sku.VersionMajor}.{sku.VersionMinor}{(sku.FreeThreaded ? "t" : "")}{(sku.Debug ? "d" : "")}");
                 context.Environment
                        .WithVirtualEnvironment(Path.Join(Environment.CurrentDirectory, "python", venv))
                        .WithPipInstaller();
             })
    {
        Installer = Services.GetRequiredService<IPythonPackageInstaller>();
    }

    public IPythonPackageInstaller Installer { get; }
}

[Collection(PythonEnvironmentCollection.Name)]
public abstract class IntegrationTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
    public IPythonPackageInstaller Installer { get; } = fixture.Installer;
}
