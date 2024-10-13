namespace Integration.Tests;

public class ReloadTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestModule_Reload()
    {
        var testModule = Env.TestChanges();
        Assert.Equal(52, testModule.TestNumber());
        // Change test_changes.py to return 42
        File.WriteAllText(
            Path.Join(Environment.CurrentDirectory, "python", "test_changes.py"),
            """
            def test_number() -> int:
                return 42

            """
        );

        testModule.ReloadModule();
        Assert.Equal(42, testModule.TestNumber());
    }
}
