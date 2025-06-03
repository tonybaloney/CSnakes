namespace RedistributablePython.Tests;

public class BasicTests: RedistributablePythonTestBase
{
    [Fact]
    public void TestSimpleRedistributableImport()
    {
        var testModule = Env.TestRedistImports();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
