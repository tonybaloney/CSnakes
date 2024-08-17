using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Python;
public class PyObjectTests : RuntimeTestBase
{
    private TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));

    [Fact]
    public void TestToString()
    {
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFromString("Hello, World!") as PyObject;
            Assert.NotNull(pyObj);
            Assert.Equal("Hello, World!", pyObj!.ToString());
        }
    }

    [Fact]
    public void TestObjectType()
    {
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFromString("Hello, World!") as PyObject;
            Assert.NotNull(pyObj);
            Assert.Equal("<class 'str'>", pyObj!.GetPythonType().ToString());
        }
    }

    [Fact]
    public void TestObjectGetAttr()
    {
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFromString("Hello, World!") as PyObject;
            Assert.NotNull(pyObj);
            Assert.True(pyObj!.HasAttr("__doc__"));
            using PyObject? pyObjDoc = pyObj!.GetAttr("__doc__");
            Assert.NotNull(pyObjDoc);
            Assert.Contains("Create a new string ", pyObjDoc!.ToString());
        }
    }

    [Fact]
    public void TestObjectGetRepr()
    {
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFromString("hello") as PyObject;
            Assert.NotNull(pyObj);
            string pyObjDoc = pyObj!.GetRepr();
            Assert.False(string.IsNullOrEmpty(pyObjDoc));
            Assert.Contains("'hello'", pyObjDoc);
        }
    }

    [Fact]
    public void CannotUnsafelyGetHandleFromDisposedPyObject()
    {
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFromString("hello") as PyObject;
            Assert.NotNull(pyObj);
            pyObj.Dispose();
            Assert.Throws<ObjectDisposedException>(() => pyObj!.ToString());
        }
    }
}