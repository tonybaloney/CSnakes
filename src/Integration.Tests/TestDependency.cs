using Integration.Tests;
using System.Runtime.InteropServices;

public class TestDependency : IntegrationTestBase
{
    [Fact]
    public void VerifyInstalledPackage()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) // TODO: Fix virtual environments on Linux
        {
            Assert.True(Env.TestDependency().TestNothing());
        }
    }
}
