using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class PyObjectTests(PythonEnvironmentFixture fixture) : RuntimeTestBase(fixture)
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
    public void TestObjectIsSmallIntegers()
    {
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
        Assert.True(obj1!.Equals(obj2));
        Assert.True(obj1 == obj2);
    }

    [Fact]
    public void TestObjectEqualsCollection()
    {
        using var obj1 = PyObject.From(new[] { "Hello!", "World!" });
        using var obj2 = PyObject.From(new[] { "Hello!", "World!" });
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
        using var obj1 = PyObject.From(o1);
        using var obj2 = PyObject.From(o2);
        Assert.True(obj1!.NotEquals(obj2));
        Assert.True(obj1 != obj2);
    }

    [Fact]
    public void TestObjectNotEqualsCollection()
    {
        using var obj1 = PyObject.From(new[] { "Hello!", "World!" });
        using var obj2 = PyObject.From(new[] { "Hello?", "World?" });
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
        using var obj1 = o1 is null ? null : PyObject.From(o1);
        using var obj2 = o2 is null ? null : PyObject.From(o2);
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
        using var obj1 = o1 is null ? null : PyObject.From(o1);
        using var obj2 = o2 is null ? null : PyObject.From(o2);
        Assert.Equal(expectedLT, obj1 <= obj2);
        Assert.Equal(expectedGT, obj1 >= obj2);
    }

    public static TheoryData<bool, object?> BooleanishTestCases(bool yes) => new()
    {
        { yes,  /* True      */ true },
        { !yes, /* False     */ false },
        { !yes, /* None      */ null },
        { yes,  /* -42       */ -42 },
        { !yes, /* 0         */ 0 },
        { yes,  /* 42        */ 42 },
        { !yes, /* ""        */ "" },
        { yes,  /* "foobar"  */ "foobar" },
        { !yes, /* []        */ Array.Empty<int>() },
        { yes,  /* [42]      */ new[] { 42 } },
        { !yes, /* {}        */ new Dictionary<int, string>() },
        { yes,  /* { 0: "" } */ new Dictionary<int, string> { [0] = "" } },
        { !yes, /* ()        */ new ValueTuple() },
        { yes,  /* (0,)      */ new ValueTuple<int>(0) },
    };

    /// <summary>
    /// Exercises <see cref="PyObject.op_True"/>.
    /// </summary>
    [Theory]
    [MemberData(nameof(BooleanishTestCases), true)]
    public void TestTruthy(bool expected, object? input)
    {
        using var obj = PyObject.From(input);
        if (obj)
            Assert.True(expected);
        else
            Assert.False(expected);
    }

    /// <summary>
    /// Exercises <see cref="PyObject.op_LogicalNot"/>.
    /// </summary>
    [Theory]
    [MemberData(nameof(BooleanishTestCases), false)]
    public void TestFalsy(bool expected, object? input)
    {
        using var obj = PyObject.From(input);
        var result = !obj;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CallWithNonStringKeywordThrowsException()
    {
        using var str = PyObject.From("Hello, World!");
        using var encode = str.GetAttr("encode");
        using var encoding = PyObject.From("utf-8");
        void Act() => encode.Call([], [new KeywordArg(PyObject.Zero, encoding)]).Dispose();
        var invocationException = Assert.Throws<PythonInvocationException>(Act);
        var runtimeException = Assert.IsType<PythonRuntimeException>(invocationException.InnerException);
        Assert.Equal("keywords must be strings", runtimeException.Message);
    }

    [Fact]
    public void CallWithKeywordsPassesEachToItsParameter()
    {
        const string source = "def f(a, b=0, *, c=0, **rest):\n    return f'{a} {b} {c} {sorted(rest.items())}'\n";
        using var module = Import.ImportModule("call_with_keywords", source, Environment.CurrentDirectory);
        using var f = module.GetAttr("f");
        using var one = PyObject.From(1);
        using var two = PyObject.From(2);
        using var three = PyObject.From(3);
        using var four = PyObject.From(4);
        using var b = PyObject.From("b");
        using var c = PyObject.From("c");
        using var d = PyObject.From("d");
        using var result = f.Call([one], [new KeywordArg(c, three), new KeywordArg(b, two), new KeywordArg(d, four)]);
        Assert.Equal("1 2 3 [('d', 4)]", result.ToString());
    }

    [Fact]
    public void CallWithKeywordsOfABuiltin()
    {
        using var builtins = Import.ImportModule("builtins");
        using var sorted = builtins.GetAttr("sorted");
        using var items = PyObject.From(new long[] { 2, 3, 1 });
        using var reverse = PyObject.From("reverse");
        using var result = sorted.Call([items], [new KeywordArg(reverse, PyObject.True)]);
        Assert.Equal("[3, 2, 1]", result.ToString());
    }

    [Fact]
    public void CallWithKeywordsOfAType()
    {
        using var builtins = Import.ImportModule("builtins");
        using var dict = builtins.GetAttr("dict");
        using var a = PyObject.From("a");
        using var b = PyObject.From("bee");
        using var result = dict.Call([], [new KeywordArg(a, PyObject.One), new KeywordArg(b, PyObject.Zero)]);
        Assert.Equal("{'a': 1, 'bee': 0}", result.ToString());
    }

    [Theory]
    [InlineData("a")]   // one character: the same str object each time
    [InlineData("key")] // two str objects of one value
    public void CallWithAKeywordTwiceKeepsTheLastValue(string name)
    {
        using var builtins = Import.ImportModule("builtins");
        using var dict = builtins.GetAttr("dict");
        using var first = PyObject.From(name);
        using var second = PyObject.From(name);
        using var result = dict.Call([], [new KeywordArg(first, PyObject.One), new KeywordArg(second, PyObject.Zero)]);
        Assert.Equal($"{{'{name}': 0}}", result.ToString());
    }

    [Fact]
    public void CallWithManyKeywords()
    {
        using var builtins = Import.ImportModule("builtins");
        using var dict = builtins.GetAttr("dict");
        var names = Enumerable.Range(0, 20).Select(i => PyObject.From($"k{i}")).ToArray();
        var values = Enumerable.Range(0, 20).Select(i => PyObject.From((long)i)).ToArray();
        try
        {
            using var result = dict.Call([], [.. names.Zip(values, (n, v) => new KeywordArg(n, v))]);
            Assert.Equal(20, result.As<IReadOnlyDictionary<string, long>>().Count);
            Assert.Equal(19, result.As<IReadOnlyDictionary<string, long>>()["k19"]);
        }
        finally
        {
            foreach (var o in names.Concat(values))
                o.Dispose();
        }
    }

    [Fact]
    public void CallWithAKeywordGivenByPositionToo()
    {
        const string source = "def f(a, b=0):\n    return a + b\n";
        using var module = Import.ImportModule("call_with_keyword_twice", source, Environment.CurrentDirectory);
        using var f = module.GetAttr("f");
        using var a = PyObject.From("a");
        void Act() => f.Call([PyObject.One], [new KeywordArg(a, PyObject.One)]).Dispose();
        var invocationException = Assert.Throws<PythonInvocationException>(Act);
        Assert.Contains("multiple values for argument 'a'", invocationException.InnerException!.Message);
    }
}
