using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class PyDictionaryTests : RuntimeTestBase
{
    [Fact]
    public void TestIndex()
    {
        var testDict = new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyObject = PyObject.From(testDict);
        var pyDict = pyObject.As<IReadOnlyDictionary<string, string>>();
        Assert.NotNull(pyDict);
        Assert.Equal("World?", pyDict["Hello"]);
        Assert.Equal("Bar", pyDict["Foo"]);
        // Try twice 
        Assert.Equal("World?", pyDict["Hello"]);
    }

    [Fact]
    public void TestKeys()
    {
        var testDict = new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyObject = PyObject.From(testDict);
        var pyDict = pyObject.As<IReadOnlyDictionary<string, string>>();
        Assert.NotNull(pyDict);
        Assert.Equal(2, pyDict.Count);
        Assert.Contains("Hello", pyDict.Keys);
        Assert.Contains("Foo", pyDict.Keys);
    }

    [Fact]
    public void TestKeysEnumeratorFunctionsDespiteEarlyDictionaryDisposal()
    {
        var testDict = new Dictionary<string, string>
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyDict = PyObject.From(testDict).As<IReadOnlyDictionary<string, string>>();

        using var e = pyDict.Keys.GetEnumerator();
        ((IDisposable)pyDict).Dispose();

        Assert.True(e.MoveNext());
        Assert.Equal("Hello", e.Current);

        Assert.True(e.MoveNext());
        Assert.Equal("Foo", e.Current);

        Assert.False(e.MoveNext());
    }

    [Fact]
    public void TestValues()
    {
        var testDict = new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyObject = PyObject.From(testDict);
        var pyDict = pyObject.As<IReadOnlyDictionary<string, string>>();
        Assert.NotNull(pyDict);
        Assert.Equal(2, pyDict.Count);
        Assert.Contains("World?", pyDict.Values);
        Assert.Contains("Bar", pyDict.Values);
    }

    [Fact]
    public void TestValuesEnumeratorFunctionsDespiteEarlyDictionaryDisposal()
    {
        var testDict = new Dictionary<string, string>
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyDict = PyObject.From(testDict).As<IReadOnlyDictionary<string, string>>();

        using var e = pyDict.Values.GetEnumerator();
        ((IDisposable)pyDict).Dispose();

        Assert.True(e.MoveNext());
        Assert.Equal("World?", e.Current);

        Assert.True(e.MoveNext());
        Assert.Equal("Bar", e.Current);

        Assert.False(e.MoveNext());
    }

    [Fact]
    public void TestCount()
    {
        var testDict = new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyObject = PyObject.From(testDict);
        var pyDict = pyObject.As<IReadOnlyDictionary<string, string>>();
        Assert.NotNull(pyDict);
        Assert.Equal(2, pyDict.Count);
    }

    [Fact]
    public void TestContainsKey()
    {
        var testDict = new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyObject = PyObject.From(testDict);
        var pyDict = pyObject.As<IReadOnlyDictionary<string, string>>();
        Assert.NotNull(pyDict);
        Assert.True(pyDict.ContainsKey("Hello"));
        Assert.True(pyDict.ContainsKey("Foo"));
        Assert.False(pyDict.ContainsKey("Bar"));
    }

    [Fact]
    public void TestTryGetValue()
    {
        var testDict = new Dictionary<string, string>()
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyObject = PyObject.From(testDict);
        var pyDict = pyObject.As<IReadOnlyDictionary<string, string>>();
        Assert.NotNull(pyDict);
        Assert.True(pyDict.TryGetValue("Hello", out var value));
        Assert.Equal("World?", value);
        Assert.True(pyDict.TryGetValue("Foo", out value));
        Assert.Equal("Bar", value);
        Assert.False(pyDict.TryGetValue("Bar", out _));
    }

    [Fact]
    public void TestGetEnumeratorFunctionsDespiteEarlyDictionaryDisposal()
    {
        var testDict = new Dictionary<string, string>
        {
            ["Hello"] = "World?",
            ["Foo"] = "Bar"
        };
        var pyDict = PyObject.From(testDict).As<IReadOnlyDictionary<string, string>>();

        using var e = pyDict.GetEnumerator();
        ((IDisposable)pyDict).Dispose();

        Assert.True(e.MoveNext());
        Assert.Equal(new KeyValuePair<string, string>("Hello", "World?"), e.Current);

        Assert.True(e.MoveNext());
        Assert.Equal(new KeyValuePair<string, string>("Foo", "Bar"), e.Current);

        Assert.False(e.MoveNext());
    }
}
