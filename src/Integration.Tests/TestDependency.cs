namespace Integration.Tests;

public class TestDependency(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void VerifyInstalledPackage()
    {
        Assert.True(Env.TestDependency().TestNothing());
    }

    [Fact]
    public void VerifyFastEmbed()
    {
        var mod = Env.TestDependency();
        mod.Initialize();
        Assert.True(mod.GenerateQueryEmbedding("1234 hello world").Any());
    }
}
