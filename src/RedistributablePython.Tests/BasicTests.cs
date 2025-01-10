namespace RedistributablePython.Tests;

public class BasicTests : RedistributablePythonTestBase
{
    [Fact]
    public void TestSimpleImport()
    {
        var testModule = Env.TestSimple();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
