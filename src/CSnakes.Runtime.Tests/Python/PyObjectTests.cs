using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class PyObjectTests : RuntimeTestBase
{
    [Fact]
    public void TestToString()
    {
        using PythonObject? pyObj = PythonObject.From("Hello, World!");
        Assert.NotNull(pyObj);
        Assert.Equal("Hello, World!", pyObj!.ToString());
    }

    [Fact]
    public void TestObjectType()
    {
        using PythonObject? pyObj = PythonObject.From("Hello, World!");
        Assert.NotNull(pyObj);
        Assert.Equal("<class 'str'>", pyObj!.GetPythonType().ToString());
    }

    [Fact]
    public void TestObjectGetAttr()
    {
        using PythonObject? pyObj = PythonObject.From("Hello, World!");
        Assert.NotNull(pyObj);
        Assert.True(pyObj!.HasAttr("__doc__"));
        using PythonObject? pyObjDoc = pyObj!.GetAttr("__doc__");
        Assert.NotNull(pyObjDoc);
        Assert.Contains("Create a new string ", pyObjDoc!.ToString());
    }

    [Fact]
    public void TestObjectGetRepr()
    {
        using PythonObject? pyObj = PythonObject.From("hello");
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
            using PythonObject? pyObj = PythonObject.From("hello");
            Assert.NotNull(pyObj);
            pyObj.Dispose();
            Assert.Throws<ObjectDisposedException>(() => pyObj!.ToString());
        }
    }

    [Fact]
    public void TestObjectIsNone()
    {
        var obj1 = PythonObject.None;
        var obj2 = PythonObject.None;
        Assert.True(obj2.Is(obj2));
    }

    [Fact]
    public void TestObjectIsSmallIntegers()
    {
        // Small numbers are the same object in Python, weird implementation detail.
        var obj1 = PythonObject.From(42);
        var obj2 = PythonObject.From(42);
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
        using var obj1 = PythonObject.From(o1);
        using var obj2 = PythonObject.From(o2);
        Assert.True(obj1!.Equals(obj2));
        Assert.True(obj1 == obj2);
    }

    [Fact]
    public void TestObjectEqualsCollection()
    {
        using var obj1 = PythonObject.From<IEnumerable<string>>(["Hello!", "World!"]);
        using var obj2 = PythonObject.From<IEnumerable<string>>(["Hello!", "World!"]);
        Assert.True(obj1!.Equals(obj2));
        Assert.True(obj1 == obj2);
    }

    [InlineData(null, true)]
    [InlineData(42, 44)]
    [InlineData(42123434, 421234)]
    [InlineData("Hello!", "Hello?")]
    [InlineData(3, 3.2)]
    [Theory]
    public void TestObjectNotEquals(object? o1, object? o2)
    {
        using var obj1 = PythonObject.From(o1);
        using var obj2 = PythonObject.From(o2);
        Assert.True(obj1!.NotEquals(obj2));
        Assert.True(obj1 != obj2);
    }

    [Fact]
    public void TestObjectNotEqualsCollection()
    {
        using var obj1 = PythonObject.From<IEnumerable<string>>(["Hello!", "World!"]);
        using var obj2 = PythonObject.From<IEnumerable<string>>(["Hello?", "World?"]);
        Assert.True(obj1!.NotEquals(obj2));
        Assert.True(obj1 != obj2);
    }

    [InlineData(null, null, false, false)]
    [InlineData(0, null, false, false)]
    [InlineData(null, 0, false, false)]
    [InlineData(long.MaxValue, 0, false, true)]
    [InlineData(long.MinValue, 0, true, false)]
    [InlineData(0, long.MaxValue, true, false)]
    [InlineData(0, long.MinValue, false, true)]
    [InlineData((long)-1, 1, true, false)]
    [InlineData(1, (long)-1, false, true)]
    [InlineData("a", "b", true, false)]
    [InlineData("b", "a", false, true)]
    [InlineData(3.0, 3.2, true, false)]
    [Theory]
    public void TestObjectStrictInequality(object? o1, object? o2, bool expectedLT, bool expectedGT)
    {
        using var obj1 = o1 is null ? null : PythonObject.From(o1);
        using var obj2 = o2 is null ? null : PythonObject.From(o2);
        Assert.Equal(expectedLT, obj1 < obj2);
        Assert.Equal(expectedGT, obj1 > obj2);
    }

    [InlineData(null, null, true, true)]
    [InlineData(0, null, false, false)]
    [InlineData(null, 0, false, false)]
    [InlineData(long.MaxValue, 0, false, true)]
    [InlineData(long.MinValue, 0, true, false)]
    [InlineData(0, long.MaxValue, true, false)]
    [InlineData(0, long.MinValue, false, true)]
    [InlineData((long)-1, 1, true, false)]
    [InlineData(1, (long)-1, false, true)]
    [InlineData("a", "b", true, false)]
    [InlineData("b", "a", false, true)]
    [InlineData(3.0, 3.2, true, false)]
    [InlineData("b", "b", true, true)]
    [InlineData(3.0, 3.0, true, true)]
    [Theory]
    public void TestObjectNotStrictInequality(object? o1, object? o2, bool expectedLT, bool expectedGT)
    {
        using var obj1 = o1 is null ? null : PythonObject.From(o1);
        using var obj2 = o2 is null ? null : PythonObject.From(o2);
        Assert.Equal(expectedLT, obj1 <= obj2);
        Assert.Equal(expectedGT, obj1 >= obj2);
    }
}