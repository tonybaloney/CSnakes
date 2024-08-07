using CSnakes.Runtime;

namespace Integration.Tests;
public partial class EndToEndTests
{
    [Fact]
    public void TestDefault_StrArg()
    {
        var testDefaults = testEnv.Env.TestDefaults();
        Assert.Equal("hello", testDefaults.TestDefaultStrArg());
    }

    [Fact]
    public void TestDefault_IntArg()
    {
        var testDefaults = testEnv.Env.TestDefaults();
        Assert.Equal(1337, testDefaults.TestDefaultIntArg());
    }

    [Fact]
    public void TestDefault_FloatArg()
    {
        var testDefaults = testEnv.Env.TestDefaults();
        Assert.Equal(-1, testDefaults.TestDefaultFloatArg());
    }
}
