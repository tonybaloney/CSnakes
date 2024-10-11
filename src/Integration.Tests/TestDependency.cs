namespace Integration.Tests;

public class TestDependency(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void VerifyInstalledPackage()
    {
        Assert.True(Env.TestDependency().TestNothing());
    }

    [Fact]
    public void TestModule_Reload()
    {
        var testModule = Env.TestChanges();
        Assert.Equal(52, testModule.TestNumber());
        // Change test_changes.py to return 42
        File.WriteAllText(
            Path.Join(Environment.CurrentDirectory, "python", "test_changes.py"),
            "def test_number() -> int:\n    return 42\n"
        );

        testModule.ReloadModule();
        Assert.Equal(42, testModule.TestNumber());
    }
}
