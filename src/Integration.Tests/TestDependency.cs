using Integration.Tests;

public class TestDependency : IntegrationTestBase
{
    [Fact]
    public void VerifyInstalledPackage()
    {
        var module = Env.TestDependency();
        Assert.True(module.TestNothing());
    }
}
