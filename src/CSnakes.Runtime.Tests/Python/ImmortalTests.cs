using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class ImmortalTests : RuntimeTestBase
{
    [Fact]
    public void TestZero()
    {
        Assert.Equal("0", PyObject.Zero.ToString());
        Assert.Equal("0", PyObject.Zero.GetRepr());
        Assert.False(PyObject.Zero.IsNone());
        Assert.Equal(PyObject.Zero, PyObject.Zero.Clone());
        using var zero = PyObject.From(0);
        Assert.Equal(PyObject.Zero, zero);
    }

    [Fact]
    public void TestNegativeOne()
    {
        Assert.Equal("-1", PyObject.NegativeOne.ToString());
        Assert.Equal("-1", PyObject.NegativeOne.GetRepr());
        Assert.False(PyObject.NegativeOne.IsNone());
        Assert.Equal(PyObject.NegativeOne, PyObject.NegativeOne.Clone());
        using var negativeOne = PyObject.From(-1);
        Assert.Equal(PyObject.NegativeOne, negativeOne);
    }

    [Fact]
    public void TestOne()
    {
        Assert.Equal("1", PyObject.One.ToString());
        Assert.Equal("1", PyObject.One.GetRepr());
        Assert.False(PyObject.One.IsNone());
        Assert.Equal(PyObject.One, PyObject.One.Clone());
        using var one = PyObject.From(1);
        Assert.Equal(PyObject.One, one);
    }

    [Fact]
    public void TestFalse()
    {
        Assert.Equal("False", PyObject.False.ToString());
        Assert.Equal("False", PyObject.False.GetRepr());
        Assert.False(PyObject.False.IsNone());
        Assert.Equal(PyObject.False, PyObject.False.Clone());
        using var pyFalse = PyObject.From(false);
        Assert.Equal(PyObject.False, pyFalse);
    }

    [Fact]
    public void TestTrue()
    {
        Assert.Equal("True", PyObject.True.ToString());
        Assert.Equal("True", PyObject.True.GetRepr());
        Assert.False(PyObject.True.IsNone());
        Assert.Equal(PyObject.True, PyObject.True.Clone());
        using var pyTrue = PyObject.From(true);
        Assert.Equal(PyObject.True, pyTrue);
    }
}
