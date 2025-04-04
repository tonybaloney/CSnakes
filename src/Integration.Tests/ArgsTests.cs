using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace Integration.Tests;
public class ArgsTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void PositionalOnly()
    {
        var mod = Env.TestArgs();
        Assert.Equal(6, mod.PositionalOnlyArgs(1, 2, 3));
    }

    [Fact]
    public void CollectStarArgs()
    {
        var mod = Env.TestArgs();

        using (GIL.Acquire())
        {
            using PyObject arg1 = PyObject.From(3L);

            Assert.Equal(6, mod.CollectStarArgs(1, 2, [arg1]));
        }
    }

    [Fact]
    public void CollectStarStarKwargs()
    {
        var mod = Env.TestArgs();

        using (GIL.Acquire())
        {
            using PyObject arg1 = PyObject.From(3L);

            Assert.Equal(6, mod.CollectStarStarKwargs(1, 2, new Dictionary<string, PyObject> { { "c", arg1 } }));

        }
    }

    [Fact]
    public void PositionalAndKwargs()
    {
        var mod = Env.TestArgs();

        using (GIL.Acquire())
        {
            using PyObject arg1 = PyObject.From(3L);

            Assert.Equal(9, mod.PositionalAndKwargs(a: 1, b: 2, c: 3, kwargs: new Dictionary<string, PyObject> { { "d", arg1 } }));
        }
    }

    [Fact]
    public void KeywordOnly()
    {
        var mod = Env.TestArgs();
        Assert.Equal(6, mod.KeywordOnlyArgs(1, b: 2, c: 3));
    }

    [Fact]
    public void CollectStarArgsAndKeywordOnlyArgs()
    {
        var mod = Env.TestArgs();

        using (GIL.Acquire())
        {
            using PyObject arg1 = PyObject.From(3L);

            Assert.Equal(9, mod.CollectStarArgsAndKeywordOnlyArgs(a: 1, b: 2, c: 3, args: [arg1]));
        }
    }
}
