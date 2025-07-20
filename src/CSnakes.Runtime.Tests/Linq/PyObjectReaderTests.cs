using CSnakes.Linq;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Linq;

public class PyObjectReaderTests : RuntimeTestBase
{
    [Theory]
    [InlineData(42)]
    [InlineData(4242)]
    public void Return(int input)
    {
        var reader = PyObjectReader.Return(input);
        using var obj = PyObject.From("foobar");

        var result = reader.Read(obj);

        Assert.Equal(input, result);
    }

    [Fact]
    public void Clone()
    {
        const string input = "foobar";
        using var result = PyObjectReader.Clone.Read(PyObject.From(input));
        Assert.Equal(input, result.ToString());
    }

    [Theory]
    [InlineData("foobar", false)]
    [InlineData("__str__", true)]
    public void HasAttr(string name, bool expected)
    {
        using var obj = PyObject.From(42);
        var result = PyObjectReader.HasAttr(name).Read(obj);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAttr()
    {
        using var obj = PyObject.From(42);
        var result = PyObjectReader.GetAttr("__class__").Read(obj);
        Assert.Equal("<class 'int'>", result.ToString());
    }

    public class BooleanTests : RuntimeTestBase
    {
        [Fact]
        public void ReturnsTrueForTrue()
        {
            var result = PyObjectReader.Boolean.Read(PyObject.True);
            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalseForFalse()
        {
            var result = PyObjectReader.Boolean.Read(PyObject.False);
            Assert.False(result);
        }
    }

    public class StringTests : RuntimeTestBase
    {
        [Fact]
        public void ReturnsString()
        {
            const string input = "foobar";
            using var obj = PyObject.From(input);

            var result = PyObjectReader.String.Read(obj);

            Assert.Equal(input, result);
        }

        [Fact]
        public void ThrowsTypeErrorForOtherType()
        {
            var obj = PyObject.From(42);

            void Act() => PyObjectReader.String.Read(obj);

            var ex = Assert.Throws<PythonInvocationException>(Act);
            Assert.Equal("TypeError", ex.PythonExceptionType);
        }
    }

    public class TupleTests : RuntimeTestBase
    {
        [Fact]
        public void ReturnsTuple()
        {
            var reader = PyObjectReader.Tuple(PyObjectReader.Int64, PyObjectReader.String);
            var input = (42, "foobar");
            using var obj = PyObject.From(input);

            var result = reader.Read(obj);

            Assert.Equal(input, result);
        }
    }

    [Fact]
    public void Select()
    {
        var obj = PyObject.From(42);

        var reader =
            from x in PyObjectReader.Int64
            select x * 2;

        var result = reader.Read(obj);

        Assert.Equal(84, result);
    }

    [Fact]
    public void SelectMany()
    {
        var len = PyObjectReader.Call("__len__", PyObjectReader.Int64);
        var tuple = PyObjectReader.Tuple(PyObjectReader.Int64, PyObjectReader.String);

        var obj = PyObject.From((42L, "foobar"));

        var reader =
            from t in tuple
            from l in len
            select new
            {
                Length = l,
                t.Item1,
                t.Item2,
            };

        var result = reader.Read(obj);

        Assert.Equal(new { Length = 2L, Item1 = 42L, Item2 = "foobar" }, result);
    }

    [Fact]
    public void Zip()
    {
        var len = PyObjectReader.Call("__len__", PyObjectReader.Int64);
        var tuple = PyObjectReader.Tuple(PyObjectReader.Int64, PyObjectReader.String);

        var obj = PyObject.From((42L, "foobar"));

        var reader =
            from e in tuple.Zip(len)
            select new
            {
                Length = e.Second,
                e.First.Item1,
                e.First.Item2,
            };

        var result = reader.Read(obj);

        Assert.Equal(new { Length = 2L, Item1 = 42L, Item2 = "foobar" }, result);
    }
}

