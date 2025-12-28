using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class RunTests : RuntimeTestBase
{
    [Fact]
    public void TestSimpleString()
    {
        using var result = env.ExecuteExpression("1+1");
        Assert.Equal("2", result.ToString());
    }

    [Fact]
    public void TestBadString()
    {
        Assert.Throws<PythonInvocationException>(() => env.ExecuteExpression("1+"));
    }

    [Fact]
    public void TestSimpleStringWithLocals()
    {
        var locals = new Dictionary<string, PyObject>
        {
            ["a"] = PyObject.From(101)
        };
        using var result = env.ExecuteExpression("a+1", locals);
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
        using var result = env.ExecuteExpression("a+b+1", locals, globals);
        Assert.Equal("202", result.ToString());
    }

    [Fact]
    public void TestMultilineInput()
    {
        var c = """
a = 101
b = c + a
""";
        var locals = new Dictionary<string, PyObject>
        {
            ["c"] = PyObject.From(101)
        };
        var globals = new Dictionary<string, PyObject>
        {
            ["d"] = PyObject.From(100)
        };
        using var result = env.Execute(c, locals, globals);
        Assert.Equal("None", result.ToString());
    }

    [Fact]
    public void TestExecuteWithLocalsUpdatesDict()
    {
        var code = """c = a * 2; b += 1""";
        var locals = new Dictionary<string, PyObject>
        {
            ["a"] = PyObject.From(101)
        };
        var globals = new Dictionary<string, PyObject>
        {
            ["b"] = PyObject.From(100)
        };
        using var result = env.Execute(code, locals, globals);
        Assert.Equal("None", result.ToString());
        Assert.Equal(202, locals["c"].As<long>());
        Assert.Equal(101, locals["b"].As<long>());
    }
}
