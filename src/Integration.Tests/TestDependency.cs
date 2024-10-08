using System.Runtime.InteropServices;

namespace Integration.Tests;

public class TestDependency(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void VerifyInstalledPackage()
    {
        Assert.True(Env.TestDependency().TestNothing());
    }
}