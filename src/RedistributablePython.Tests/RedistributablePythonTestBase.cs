using CSnakes.Runtime.Tests;
using Microsoft.Extensions.Logging;

namespace RedistributablePython.Tests;

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
[CollectionDefinition(Name)]
public sealed class PythonEnvironmentCollection : ICollectionFixture<PythonEnvironmentFixture>
{
    public const string Name = "PythonEnvironment";
}

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
public sealed class PythonEnvironmentFixture(RedistributablePythonSkuFixture sku) :
    RedistributablePythonEnvironmentFixture(
        sku, home: Path.Join(Environment.CurrentDirectory, "python"),
        context =>
        {
            _ = context.Environment
                       .DisableSignalHandlers()
                       .WithUvInstaller()
                       .WithVirtualEnvironment(sku.VirtualEnvironmentPath)
                       .CapturePythonLogs();

            context.Logging
                   .SetMinimumLevel(LogLevel.Debug)
                   .AddFilter(_ => true);
        });

[Collection(PythonEnvironmentCollection.Name)]
public class RedistributablePythonTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
}
