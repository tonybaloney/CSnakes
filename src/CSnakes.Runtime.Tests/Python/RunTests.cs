using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class RunTests : RuntimeTestBase
{
    [Fact]
    public void TestSimpleString()
    {
        using var result = env.Execute("1+1");
        Assert.Equal("2", result.ToString());
    }

    [Fact]
    public void TestBadString()
    {
        Assert.Throws<PythonInvocationException>(() => env.Execute("1+"));
    }

    [Fact]
    public void TestSimpleStringWithLocals()
    {
        var locals = new Dictionary<string, PyObject>
        {
            ["a"] = PyObject.From(101)
        };
        using var result = env.Execute("a+1", locals);
        Assert.Equal("102", result.ToString());
    }

    [Fact]
    public void TestSimpleStringWithLocalsAndGlobals()
    {
        var locals = new Dictionary<string, PyObject>
        {
            ["a"] = PyObject.From(101)
        };
        var globals = new Dictionary<string, PyObject>
        {
            ["b"] = PyObject.From(100)
        };
        using var result = env.Execute("a+b+1", locals);
        Assert.Equal("202", result.ToString());
    }
}
