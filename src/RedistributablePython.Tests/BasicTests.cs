using Xunit.Abstractions;

namespace RedistributablePython.Tests;

public class BasicTests(ITestOutputHelper testOutputHelper) : RedistributablePythonTestBase(testOutputHelper)
{
    [Fact]
    public void TestSimpleImport()
    {
        var testModule = Env.TestSimple();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
