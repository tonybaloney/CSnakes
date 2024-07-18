using Python.Generated;
using Python.Runtime;
using System.Collections.Immutable;
using System.Reflection;

namespace Integration.Tests;

public class BasicTest(TestEnvironment testEnv) : IClassFixture<TestEnvironment>
{
    TestEnvironment testEnv = testEnv;

    [Fact]
    public void TestBasic()
    {

        var testModule = testEnv.Env.TestBasic();

        Assert.Equal(4.3, testModule.TestIntFloat(4, 0.3));
        Assert.Equal(4.3, testModule.TestFloatInt(0.3, 4));
        Assert.Equal(4.3, testModule.TestFloatFloat(0.3, 4.0));
        Assert.Equal(6, testModule.TestIntInt(4, 2));
        Assert.Equal([1, 2, 3], testModule.TestListOfInts([1, 2, 3]));
        Assert.Equal("hello world", testModule.TestTwoStrings("hello ", "world"));
        Assert.Equal(["hello", "world", "this", "is", "a", "test"], testModule.TestTwoListsOfStrings(["hello", "world"], ["this", "is", "a", "test"]));


    }

    [Fact]
    public void TestFalseReturnTypes() { 
        var falseReturns = testEnv.Env.TestFalseReturns();

        // TODO: Standardise the exception that gets raised when the response type is invalid.
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsInt());
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsFloat());
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsList());
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsTuple());
        Assert.Throws<TargetInvocationException>(() => falseReturns.TestTupleActuallyReturnsInt());
        Assert.Throws<TargetInvocationException>(() => falseReturns.TestTupleActuallyReturnsFloat());
        Assert.Throws<TargetInvocationException>(() => falseReturns.TestTupleActuallyReturnsList());
    }

    [Fact]
    public void TestDefaults()
    {
        var testDefaults = testEnv.Env.TestDefaults();

        Assert.Equal("hello", testDefaults.TestDefaultStrArg());
        Assert.Equal(1337, testDefaults.TestDefaultIntArg());
        Assert.Equal(-1, testDefaults.TestDefaultFloatArg());
    }

    [Fact]
    public void TestDicts()
    {
        var testDicts = testEnv.Env.TestDicts();

        IReadOnlyDictionary<string, long> testDict = new Dictionary<string, long> { { "hello", 1 }, { "world", 2 } };
        var result = testDicts.TestDictStrInt(testDict);
        Assert.Equal(1, result["hello"]);
        Assert.Equal(2, result["world"]);
    }
}