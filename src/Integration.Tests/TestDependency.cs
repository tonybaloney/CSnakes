using System.Threading.Tasks;

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

    [Fact]
    async public Task VerifyRuntimeDependency()
    {
        // attrs==25.3.0
        await Installer.InstallPackage("attrs==25.3.0");
        Assert.True(Env.TestDependency().TestAttrsFunction());
    }
}
