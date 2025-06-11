using CSnakes.Runtime.Python;

namespace Integration.Tests;
public class OverloadTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void SupportedTypeOverloading()
    {
        var mod = Env.TestOverload();
        Assert.Equal(12, mod.TestOverloadSupportedType(12));
        Assert.Equal(1.2, mod.TestOverloadSupportedType(1.2));
        Assert.Equal(PyObject.None, mod.TestOverloadSupportedType(PyObject.None));
    }

}
