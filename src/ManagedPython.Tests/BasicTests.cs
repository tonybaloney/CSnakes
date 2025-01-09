using ManagedPython.Tests;

namespace ManagedPython.Tests;

public class BasicTests : ManagedPythonTestBase
{
    [Fact]
    public void TestSimpleImport()
    {
        var testModule = Env.TestSimple();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
