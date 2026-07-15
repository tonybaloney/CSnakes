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

    [Fact]
    public void Test_ExecPrefixes()
    {
        var mod = Env.TestSys();

        string execPrefix = mod.TestSysExecPrefix();
        Assert.False(string.IsNullOrEmpty(execPrefix));
        Assert.Equal(mod.TestSysBasePrefix(), execPrefix);
    }

    [Fact]
    public void Test_SysPrefix()
    {
        var mod = Env.TestSys();
        string prefix = mod.TestSysPrefix();
        Assert.False(string.IsNullOrEmpty(prefix));
    }
}
