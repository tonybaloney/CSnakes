using CSnakes.Runtime.Python;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Tests;

public class BasicTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
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
        Assert.Equal("raboof"u8, testModule.TestBytes("foobar"u8));
    }

    [Fact]
    public async Task TestBasic_TestBytesAsync()
    {
        var testModule = Env.TestBasic();
        var actual = await testModule.TestBytesAsync("foobar"u8, TestContext.Current.CancellationToken);
        Assert.Equal("raboof"u8, actual);
    }

    [Fact]
    public void TestBasic_TestSequence()
    {
        var testModule = Env.TestBasic();
        Assert.Equal([2, 3, 4], testModule.TestSequence([1, 2, 3], 2, 5));
    }

    [Fact]
    public void TestBasic_ResultAsVariadicTuple()
    {
        var testModule = Env.TestBasic();
        var actual = testModule.TestVarTupleResult([1, 2, 3, 4], ["foo", "bar", "baz", "qux"]);
        Assert.Equal([(1, "foo"), (2, "bar"), (3, "baz"), (4, "qux")], actual);
    }

    [Fact]
    public void TestBasic_ResultAsVariadicTupleOfAny()
    {
        var testModule = Env.TestBasic();
        using var a = PyObject.From((1, "foo"));
        using var b = PyObject.From((2, "bar"));
        using var c = PyObject.From((3, "baz"));
        using var d = PyObject.From((4, "qux"));

        var actual = testModule.TestAnyVarTupleResult([a, b, c, d]);

        Assert.Equal([a, b, c, d], actual);

        foreach (var item in actual)
            item.Dispose();
    }
}
