using CSnakes.Runtime.Python;
using System.Collections.Generic;

namespace Integration.Tests;
public class ConversionTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void BooleanIsIntercepted()
    {
        Assert.True(PyObject.True.As<bool>());
        Assert.False(PyObject.False.As<bool>());
    }

    [Fact]
    public void Int64IsIntercepted()
    {
        const long value = 42;
        var result = PyObject.From(value).As<long>();
        Assert.Equal(value, result);
    }

    [Fact]
    public void DoubleIsIntercepted()
    {
        const double value = 42.0;
        var result = PyObject.From(value).As<double>();
        Assert.Equal(value, result);
    }

    [Fact]
    public void StringIsIntercepted()
    {
        const string value = "foobar";
        var result = PyObject.From(value).As<string>();
        Assert.Equal(value, result);
    }

    [Fact]
    public void ByteArrayIsIntercepted()
    {
        byte[] value = [1, 2, 3, 4];
        var result = PyObject.From(value).As<byte[]>();
        Assert.Equal(value, result);
    }

    [Fact]
    public void ListIsIntercepted()
    {
        var value = new[] { "foo", "bar", "baz" };
        var result = PyObject.From(value).As<IReadOnlyList<string>>();
        Assert.Equal(value, result);
    }

    [Fact]
    public void NestedListIsIntercepted()
    {
        var value = new string[][] { ["foo", "bar", "baz"], ["FOO", "BAR", "BAZ"] };
        var result = PyObject.From(value).As<IReadOnlyList<IReadOnlyList<string>>>();
        Assert.Equal(value, result);
    }

    [Fact]
    public void DictionaryIsIntercepted()
    {
        var value = new Dictionary<long, string> { [1] = "foo", [2] = "bar", [3] = "baz" };
        var result = PyObject.From(value).As<IReadOnlyDictionary<long, string>>();
        Assert.Equal(value, result);
    }
}
