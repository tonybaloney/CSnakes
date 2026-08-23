namespace RedistributablePython.Tests;

public class BasicTests(PythonEnvironmentFixture fixture) : RedistributablePythonTestBase(fixture)
{
    [Fact]
    public void TestSimpleRedistributableImport()
    {
        var testModule = Env.TestRedistImports();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
