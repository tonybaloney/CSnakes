using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class PyObjectTests : RuntimeTestBase
{

    [Fact]
    public void TestToString()
    {
        using PyObject? pyObj = PyObject.From("Hello, World!");
        Assert.NotNull(pyObj);
        Assert.Equal("Hello, World!", pyObj!.ToString());
    }

    [Fact]
    public void TestObjectType()
    {
        using PyObject? pyObj = PyObject.From("Hello, World!");
        Assert.NotNull(pyObj);
        Assert.Equal("<class 'str'>", pyObj!.GetPythonType().ToString());
    }

    [Fact]
    public void TestObjectGetAttr()
    {
        using PyObject? pyObj = PyObject.From("Hello, World!");
        Assert.NotNull(pyObj);
        Assert.True(pyObj!.HasAttr("__doc__"));
        using PyObject? pyObjDoc = pyObj!.GetAttr("__doc__");
        Assert.NotNull(pyObjDoc);
        Assert.Contains("Create a new string ", pyObjDoc!.ToString());
    }

    [Fact]
    public void TestObjectGetRepr()
    {
        using PyObject? pyObj = PyObject.From("hello");
        Assert.NotNull(pyObj);
        string pyObjDoc = pyObj!.GetRepr();
        Assert.False(string.IsNullOrEmpty(pyObjDoc));
        Assert.Contains("'hello'", pyObjDoc);
    }

    [Fact]
    public void CannotUnsafelyGetHandleFromDisposedPyObject()
    {
        using (GIL.Acquire())
        {
            using PyObject? pyObj = PyObject.From("hello");
            Assert.NotNull(pyObj);
            pyObj.Dispose();
            Assert.Throws<ObjectDisposedException>(() => pyObj!.ToString());
        }
    }

    [Fact]
    public void TestObjectIs()
    {
        var obj1 = PyObject.None;
        var obj2 = PyObject.None;
        Assert.True(obj2.Is(obj2));

        // Small numbers are the same object in Python
        var obj3 = PyObject.From(42);
        var obj4 = PyObject.From(42);
        Assert.True(obj3!.Is(obj4!));
    }

    [Fact]
    public void TestObjectEquals()
    {
        var obj1 = PyObject.None;
        var obj2 = PyObject.None;
        Assert.True(obj1.Equals(obj2));
        Assert.True(obj1 == obj2);

        var obj3 = PyObject.From(42);
        var obj4 = PyObject.From(42);
        Assert.True(obj3!.Equals(obj4!));
        Assert.True(obj3 == obj4);

        var obj5 = PyObject.From(42);
        var obj6 = PyObject.From(43);
        Assert.False(obj5!.Equals(obj6!));
    }
}