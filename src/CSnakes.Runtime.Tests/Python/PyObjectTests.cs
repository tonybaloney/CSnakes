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
    public void TestObjectIsNone()
    {
        var obj1 = PyObject.None;
        var obj2 = PyObject.None;
        Assert.True(obj2.Is(obj2));
    }

    [Fact]
    public void TestObjectIsSmallIntegers() {
        // Small numbers are the same object in Python, weird implementation detail. 
        var obj1 = PyObject.From(42);
        var obj2 = PyObject.From(42);
        Assert.True(obj1!.Is(obj2!));
    }

    [InlineData(null, null)]
    [InlineData(42, 42)]
    [InlineData(42123434, 42123434)]
    [InlineData("Hello!", "Hello!")]
    [InlineData(3, 3.0)]
    [Theory]
    public void TestObjectEquals(object? o1, object? o2)
    {
        using var obj1 = PyObject.From(o1);
        using var obj2 = PyObject.From(o2);
        Assert.True(obj1.Equals(obj2));
        Assert.True(obj1 == obj2);
    }

    public void TestObjectEqualsCollection()
    {
        using var obj1 = PyObject.From<IEnumerable<string>>(["Hello!", "World!"]);
        using var obj2 = PyObject.From<IEnumerable<string>>(["Hello!", "World!"]);
        Assert.False(obj1.Equals(obj2));
        Assert.False(obj1 == obj2);
    }

    [InlineData(null, true)]
    [InlineData(42, 44)]
    [InlineData(42123434, 421234)]
    [InlineData("Hello!", "Hello?")]
    [InlineData(3, 3.0)]
    [Theory]
    public void TestObjectNotEquals(object? o1, object? o2)
    {
        using var obj1 = PyObject.From(o1);
        using var obj2 = PyObject.From(o2);
        Assert.False(obj1.NotEquals(obj2));
        Assert.False(obj1 != obj2);
    }

    public void TestObjectNotEqualsCollection()
    {
        using var obj1 = PyObject.From<IEnumerable<string>>(["Hello!", "World!"]);
        using var obj2 = PyObject.From<IEnumerable<string>>(["Hello?", "World?"]);
        Assert.False(obj1.NotEquals(obj2));
        Assert.False(obj1 != obj2);
    }
}