namespace Integration.Tests;

public class BasicTests : IntegrationTestBase
{
    [Fact]
    public void TestBasic_TestIntFloat()
    {
        var testModule = Env.TestBasic();
        Assert.Equal(4.3, testModule.TestIntFloat(4, 0.3));
    }

    [Fact]
    public void TestBasic_TestFloatInt()
    {
        var testModule = Env.TestBasic();
        Assert.Equal(4.3, testModule.TestFloatInt(0.3, 4));
    }

    [Fact]
    public void TestBasic_TestFloatFloat()
    {
        var testModule = Env.TestBasic();
        Assert.Equal(4.3, testModule.TestFloatFloat(0.3, 4.0));
    }

    [Fact]
    public void TestBasic_TestIntInt()
    {
        var testModule = Env.TestBasic();
        Assert.Equal(6, testModule.TestIntInt(4, 2));
    }

    [Fact]
    public void TestBasic_TestListOfInts()
    {
        var testModule = Env.TestBasic();
        Assert.Equal([1, 2, 3], testModule.TestListOfInts([1, 2, 3]));
    }

    [Fact]
    public void TestBasic_TestTwoStrings()
    {
        var testModule = Env.TestBasic();
        Assert.Equal("hello w0rld", testModule.TestTwoStrings("hello ", "w0rld"));
    }

    [Fact]
    public void TestBasic_TestTwoListsOfStrings()
    {
        var testModule = Env.TestBasic();
        Assert.Equal(["h3llo", "worLd", "this", "is", "a", "test"], testModule.TestTwoListsOfStrings(["h3llo", "worLd"], new string[] { "this", "is", "a", "test" }));
    }

    [Fact]
    public void TestBasic_TestBytes()
    {
        var testModule = Env.TestBasic();
        Assert.Equal(new byte[] { 0x04, 0x03, 0x02, 0x01 }, testModule.TestBytes([0x01, 0x02, 0x03, 0x04]));
    }

    [Fact]
    public void TestBasic_TestSequence()
    {
        var testModule = Env.TestBasic();
        Assert.Equal([2, 3, 4], testModule.TestSequence([1, 2, 3], 2, 5));
    }
}
