namespace Integration.Tests;

public class FalseReturns : IntegrationTestBase
{
    [Fact]
    public void TestFalseReturn_StringNotInt()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsInt);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotFloat()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsFloat);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotList()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsList);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotTuple()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(falseReturns.TestStrActuallyReturnsTuple);
    }

    [Fact]
    public void TestFalseReturn_TypesTupleNotInt()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(() => falseReturns.TestTupleActuallyReturnsInt());
    }

    [Fact]
    public void TestFalseReturn_Types_TupleNotFloat()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(() => falseReturns.TestTupleActuallyReturnsFloat());
    }

    [Fact]
    public void TestFalseReturn_Types_TupleNotList()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<InvalidCastException>(() => falseReturns.TestTupleActuallyReturnsList());
    }
}
