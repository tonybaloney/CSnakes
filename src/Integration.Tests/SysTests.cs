namespace Integration.Tests;
public class SysTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void Test_SysExecutable()
    {
        var mod = Env.TestSys();

        // Test the sys.executable property
        var sysExecutable = mod.TestSysExecutable();
        Assert.Equal(Env.ExecutablePath, sysExecutable);
    }
}
