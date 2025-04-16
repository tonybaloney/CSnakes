using CSnakes.Runtime.Python;
using System.Numerics;

namespace CSnakes.Runtime.Tests.Python;

public interface IImmortalFromTestCasesContainer<T>
{
    static abstract TheoryData<T> TestCases { get; }
}

public abstract class ImmortalTestsBase<T, TTestCases>(string repr) : RuntimeTestBase
    where TTestCases : IImmortalFromTestCasesContainer<T>
{
    protected abstract PyObject Subject { get; }

    [Fact]
    public void TestString()
    {
        Assert.Equal(repr, Subject.ToString());
    }

    [Fact]
    public void TestRepr()
    {
        Assert.Equal(repr, Subject.GetRepr());
    }

    [Fact]
    public void TestIsNotNone()
    {
        Assert.False(Subject.IsNone());
    }

    [Fact]
    public void TestCloneReturnsSameInstance()
    {
        Assert.Same(Subject, Subject.Clone());
    }

    public static TheoryData<T> TestCases => TTestCases.TestCases;

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestFrom(T input)
    {
        using var value = PyObject.From(input);
        Assert.Same(Subject, value);
    }
}

public class ImmortalZeroTests() :
    ImmortalTestsBase<object, ImmortalZeroTests>("0"),
    IImmortalFromTestCasesContainer<object>
{
    protected override PyObject Subject => PyObject.Zero;

    static TheoryData<object> IImmortalFromTestCasesContainer<object>.TestCases => new() { 0, 0L, new BigInteger(0) };
}

public class ImmortalOneTests() :
    ImmortalTestsBase<object, ImmortalOneTests>("1"),
    IImmortalFromTestCasesContainer<object>
{
    protected override PyObject Subject => PyObject.One;

    static TheoryData<object> IImmortalFromTestCasesContainer<object>.TestCases => new() { 1, 1L, new BigInteger(1) };
}

public class ImmortalNegativeOneTests() :
    ImmortalTestsBase<object, ImmortalNegativeOneTests>("-1"),
    IImmortalFromTestCasesContainer<object>
{
    protected override PyObject Subject => PyObject.NegativeOne;

    static TheoryData<object> IImmortalFromTestCasesContainer<object>.TestCases => new() { -1, -1L, new BigInteger(-1) };
}

public class ImmortalTrueTests() :
    ImmortalTestsBase<bool, ImmortalTrueTests>("True"),
    IImmortalFromTestCasesContainer<bool>
{
    protected override PyObject Subject => PyObject.True;

    static TheoryData<bool> IImmortalFromTestCasesContainer<bool>.TestCases => new() { true };
}

public class ImmortalFalseTests() :
    ImmortalTestsBase<bool, ImmortalFalseTests>("False"),
    IImmortalFromTestCasesContainer<bool>
{
    protected override PyObject Subject => PyObject.False;

    static TheoryData<bool> IImmortalFromTestCasesContainer<bool>.TestCases => new() { false };
}
