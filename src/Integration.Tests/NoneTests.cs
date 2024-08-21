using CSnakes.Runtime.Python;

namespace Integration.Tests;
public class NoneTests : IntegrationTestBase
{
    [Fact]
    public void TestReturnsNoneIsNone()
    {
        var mod = Env.TestNone();
        using PyObject result = mod.ReturnsNone();
        Assert.True(result.IsNone());
    }

    [Fact]
    public void TestNullArgAsNone()
    {
        PyObject none = PyObject.None;
        Assert.True(none.IsNone());
        // Give to function
        var mod = Env.TestNone();
        Assert.True(mod.TestNoneResult(none));
    }
}
