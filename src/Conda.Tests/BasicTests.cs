namespace Conda.Tests;

public class BasicTests : CondaTestBase
{
    [Fact]
    public void TestSimpleImport()
    {
        var testModule = Env.TestSimple();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
