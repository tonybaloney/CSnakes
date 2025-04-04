namespace Integration.Tests;

public class FalseReturns(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestFalseReturn_StringNotInt()
    {
        var falseReturns = Env.TestFalseReturns();
        Assert.Throws<PythonInvocationException>(falseReturns.TestStrActuallyReturnsInt);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotFloat()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<PythonInvocationException>(falseReturns.TestStrActuallyReturnsFloat);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotList()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<PythonInvocationException>(falseReturns.TestStrActuallyReturnsList);
    }

    [Fact]
    public void TestFalseReturn_Types_StringNotTuple()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<PythonInvocationException>(falseReturns.TestStrActuallyReturnsTuple);
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

    [Fact]
    public void Test_FalseReturn_Types_IntNotFloat()
    {
        var falseReturns = Env.TestFalseReturns();
        // Python will convert the int to a float without errors
        Assert.Equal(1.0, falseReturns.TestFloatReturnsInt());
    }

    [Fact]
    public void Test_FalseReturn_Types_StrNotFloat()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<PythonInvocationException>(() => falseReturns.TestFloatReturnsStr());
    }

    [Fact]
    public void Test_FalseReturn_Types_FloatNotInt()
    {
        var falseReturns = Env.TestFalseReturns();
        if (Env.Version.StartsWith("3.9"))
        {
            // Python will convert the float to an int without errors in 3.9
            Assert.Equal(1, falseReturns.TestIntReturnsFloat());
        }
        else
        {
            Assert.Throws<PythonInvocationException>(() => falseReturns.TestIntReturnsFloat());
        }
    }

    [Fact]
    public void Test_FalseReturn_Types_StrNotInt()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<PythonInvocationException>(() => falseReturns.TestIntReturnsStr());
    }

    [Fact]
    public void Test_IntOverflowsSafely()
    {
        var falseReturns = Env.TestFalseReturns();

        Assert.Throws<PythonInvocationException>(() => falseReturns.TestIntOverflows());
    }
}
