namespace Integration.Tests;

public class ReloadTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestModule_Reload()
    {
        var testModule = Env.TestReload();
        Assert.Equal(52, testModule.TestNumber());
        // Change test_reload.py to return 42
        File.WriteAllText(
            Path.Join(Environment.CurrentDirectory, "python", "test_reload.py"),
            "def test_number() -> int:\n    return 42\n"
        );

        testModule.ReloadModule();
        Assert.Equal(42, testModule.TestNumber());
    }
}
