using CSnakes.Runtime.Python;

namespace Integration.Tests;
public class IterableTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestBuiltinIterable()
    {
        var mod = Env.TestIterable();
        using PyObject result = mod.TestBuiltinIterable();
        Assert.True(result.IsNone());
    }
}
