using Xunit.Abstractions;

namespace RedistributablePython.Tests;

public class BasicTests(Xunit.Abstractions.ITestOutputHelper testOutputHelper) : RedistributablePythonTestBase(testOutputHelper)
{
    [Fact]
    public void TestSimpleRedistributableImport()
    {
        var testModule = Env.TestRedistImports();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
