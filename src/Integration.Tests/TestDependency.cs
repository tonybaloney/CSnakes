namespace Integration.Tests;

public class TestDependency(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void VerifyInstalledPackage()
    {
        Assert.True(Env.TestDependency().TestNothing());
    }

    [Fact]
    public void VerifyPyBind11Dependency()
    {
        Assert.True(Env.TestPybind11().TestPybind11Function());
    }
}
