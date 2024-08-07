using CSnakes.Runtime;

namespace Integration.Tests;
// TODO: Standardise the exception that gets raised when the response type is invalid.
public partial class EndToEndTests
{
    [Fact]
    public void TestFalseReturn_StringNotInt()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsInt);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotFloat()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsFloat);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotList()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsList);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotTuple()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsTuple);
    }

    [Fact]
    public void TestFalseReturn_TypesTupleNotInt()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(() => falseReturns.TestTupleActuallyReturnsInt());
    }

    [Fact]
    public void TestFalseReturn_Types_TupleNotFloat()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(() => falseReturns.TestTupleActuallyReturnsFloat());
    }

    [Fact]
    public void TestFalseReturn_Types_TupleNotList()
    {
        var falseReturns = testEnv.Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(() => falseReturns.TestTupleActuallyReturnsList());
    }
}
