namespace Integration.Tests;
public class DefaultsTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
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

    [Fact]
    public void TestDefault_OptionalInt()
    {
        var testDefaults = Env.TestDefaults();
        Assert.True(testDefaults.TestOptionalInt());
    }

    [Fact]
    public void TestDefault_OptionalStr()
    {
        var testDefaults = Env.TestDefaults();
        Assert.True(testDefaults.TestOptionalStr());
    }

    [Fact]
    public void TestDefault_OptionalList()
    {
        var testDefaults = Env.TestDefaults();
        Assert.True(testDefaults.TestOptionalList());
    }

    [Fact]
    public void TestDefault_OptionalAny()
    {
        var testDefaults = Env.TestDefaults();
        Assert.True(testDefaults.TestOptionalAny());
    }

    [Fact]
    public void TestDefault_Bytes()
    {
        var testDefaults = Env.TestDefaults();
        Assert.Equal("hello"u8, testDefaults.TestDefaultBytes());
    }
}
