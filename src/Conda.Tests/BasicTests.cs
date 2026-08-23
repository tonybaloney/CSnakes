namespace Conda.Tests;

public class BasicTests(PythonEnvironmentFixture fixture) : CondaTestBase(fixture)
{
    [Fact]
    public void TestSimpleImport()
    {
        var testModule = Env.TestSimple();
        Assert.NotNull(testModule);
        testModule.TestNothing();
    }
}
