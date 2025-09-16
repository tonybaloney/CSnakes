using CSnakes.Runtime.Python;

namespace Integration.Tests;
public class TestOptional(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory]
    [InlineData(null)]
    [InlineData(42)]
    public void Int(int? input)
    {
        var mod = Env.TestOptional();
        Assert.Equal(input, mod.TestInt(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("foobar")]
    public void Str(string? input)
    {
        var mod = Env.TestOptional();
        Assert.Equal(input, mod.TestStr(input));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Object(bool @null)
    {
        var input = @null ? null : PyObject.Zero;
        var mod = Env.TestOptional();
        Assert.Equal(input, mod.TestAny(input));
    }

    [Fact]
    public void Tuple()
    {
        (int?, string) t = (10, "hello");
        var mod = Env.TestOptional();
        var result = mod.TestOptionalTuple(t);
        Assert.Equal(t, result);
    }
}
