namespace Integration.Tests;
public class DefaultsTests : IntegrationTestBase
{
    [Fact]
    public void TestDefault_StrArg()
    {
        var testDefaults = Env.TestDefaults();
        Assert.Equal("hello", testDefaults.TestDefaultStrArg());
    }

    [Fact]
    public void TestDefault_IntArg()
    {
        var testDefaults = Env.TestDefaults();
        Assert.Equal(1337, testDefaults.TestDefaultIntArg());
    }

    [Fact]
    public void TestDefault_FloatArg()
    {
        var testDefaults = Env.TestDefaults();
        Assert.Equal(-1, testDefaults.TestDefaultFloatArg());
    }
}
