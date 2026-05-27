namespace CSnakes.Runtime.Tests;

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
[CollectionDefinition(Name)]
public sealed class PythonEnvironmentCollection : ICollectionFixture<PythonEnvironmentFixture>
{
    public const string Name = "PythonEnvironment";
}

public sealed class PythonEnvironmentFixture : RedistributablePythonEnvironmentFixture;

[Collection(PythonEnvironmentCollection.Name)]
public abstract class RuntimeTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
}
