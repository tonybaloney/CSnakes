using PythonEnvironments;
using Python.Generated;
using Python.Runtime;
using System.Reflection;

namespace Integration.Tests;

public class BasicTest
{
    [Fact]
    public void TestBasic()
    {
        var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        var builder = new PythonEnvironment(
            Environment.GetEnvironmentVariable("USERPROFILE") + "\\.nuget\\packages\\python\\3.12.4\\tools",
            "3.12.4");

        using var env = builder.Build(Path.Join(Environment.CurrentDirectory, "python"));

        var testModule = env.TestBasic();

        Assert.Equal(4.3, testModule.TestIntFloat(4, 0.3));
        Assert.Equal(4.3, testModule.TestFloatInt(0.3, 4));
        Assert.Equal(4.3, testModule.TestFloatFloat(0.3, 4.0));
        Assert.Equal(6, testModule.TestIntInt(4, 2));
        Assert.Equal([1, 2, 3], testModule.TestListOfInts([1, 2, 3]));
        Assert.Equal("hello world", testModule.TestTwoStrings("hello ", "world"));
        Assert.Equal(["hello", "world", "this", "is", "a", "test"], testModule.TestTwoListsOfStrings(["hello", "world"], ["this", "is", "a", "test"]));

        var falseReturns = env.TestFalseReturns();

        // TODO: Standardise the exception that gets raised when the response type is invalid.
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsInt());
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsFloat());
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsList());
        Assert.Throws<InvalidCastException>(() => falseReturns.TestStrActuallyReturnsTuple());
        Assert.Throws<TargetInvocationException>(() => falseReturns.TestTupleActuallyReturnsInt());
        Assert.Throws<TargetInvocationException>(() => falseReturns.TestTupleActuallyReturnsFloat());
        Assert.Throws<TargetInvocationException>(() => falseReturns.TestTupleActuallyReturnsList());
    }
}