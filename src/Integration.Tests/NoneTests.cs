using CSnakes.Runtime.Python;

namespace Integration.Tests;
public class NoneTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestReturnsNoneIsNone()
    {
        var mod = Env.TestNone();
        using PythonObject result = mod.ReturnsNone();
        Assert.True(result.IsNone());
    }

    [Fact]
    public void TestNullArgAsNone()
    {
        PythonObject none = PythonObject.None;
        Assert.True(none.IsNone());
        // Give to function
        var mod = Env.TestNone();
        Assert.True(mod.TestNoneResult(none));
    }
}
