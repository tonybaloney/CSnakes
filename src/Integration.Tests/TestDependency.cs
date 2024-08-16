using System.Runtime.InteropServices;

namespace Integration.Tests;

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