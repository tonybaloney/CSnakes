namespace Integration.Tests;

public class ReloadTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    private const string originalCode = """
def reload_module() -> None:
    pass

def test_number() -> int:
    return 52
""";
    private static void ResetCode() => ReloadTests.ResetCode(originalCode);

    private static void ResetCode(string code) => File.WriteAllText(
        Path.Join(Environment.CurrentDirectory, "python", "test_reload.py"),
        code
    );

    [Fact]
    public void TestModule_Reload()
    {
        ResetCode();
        var testModule = Env.TestReload();
        ((IReloadableModuleImport)testModule).ReloadModule();
        Assert.Equal(52, testModule.TestNumber());
        // Change test_reload.py to return 42
        ResetCode(
            """
            def test_number() -> int:
                return 42

            """
        );

        ((IReloadableModuleImport)testModule).ReloadModule();
        Assert.Equal(42, testModule.TestNumber());
    }

    [Fact]
    public void TestModule_ReloadDeleteFunction()
    {
        ResetCode();
        var testModule = Env.TestReload();
        ((IReloadableModuleImport)testModule).ReloadModule();
        Assert.Equal(52, testModule.TestNumber());
        // Change test_reload.py to return 42
        ResetCode(
            """
            def _test_number() -> int:
                return 42

            """
        );

        ((IReloadableModuleImport)testModule).ReloadModule();
        Assert.Equal(52, testModule.TestNumber());
    }

    [Fact]
    public void TestModule_ReloadChangeSignature()
    {
        ResetCode();
        var testModule = Env.TestReload();
        ((IReloadableModuleImport)testModule).ReloadModule();

        Assert.Equal(52, testModule.TestNumber());
        // Change test_reload.py to return 42
        ResetCode(
            """
            def test_number() -> str:
                return "42"

            """
        );

        ((IReloadableModuleImport)testModule).ReloadModule();
        Assert.Throws<PythonInvocationException>(() => testModule.TestNumber());
    }
}
