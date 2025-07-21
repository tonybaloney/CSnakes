#pragma warning disable PRTEXP001

using CSnakes.Linq;
using System.Collections.Immutable;
using static CSnakes.Linq.PyObjectReader;

namespace Integration.Tests;
public class RichReturnTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    public sealed class FooBarBaz : IPyObjectReadable<FooBarBaz>
    {
        public long Foo { get; private init;  }
        public string? Bar { get; private init; }
        public required ImmutableArray<long> Baz { get; init; }
        public (long, string) Qux { get; private init; }
        public required ImmutableDictionary<string, long> Quux { get; init; }

        public static IPyObjectReader<FooBarBaz> Reader { get; } =
            from foo in GetAttr("foo", Int64)
            from bar in GetAttr("bar", String)
            from baz in GetAttr("baz", List(Int64, ImmutableArray.CreateRange))
            from qux in GetAttr("qux", Tuple(Int64, String))
            from quux in GetAttr("quux", Dict(Int64, ImmutableDictionary.CreateRange))
            select new FooBarBaz
            {
                Foo = foo,
                Bar = bar,
                Baz = baz,
                Qux = qux,
                Quux = quux,
            };
    }

    [Fact]
    public void Test()
    {
        var module = Env.TestRichReturn();
        var result = module.FooBarBaz();
        Assert.Equal(1, result.Foo);
        Assert.Equal("hello", result.Bar);
        Assert.Equal([1L, 2L, 3L], result.Baz);
        Assert.Equal((42L, "world"), result.Qux);
        Assert.Equal(3, result.Quux.Count);
        Assert.Equal(1, result.Quux["foo"]);
        Assert.Equal(2, result.Quux["bar"]);
        Assert.Equal(3, result.Quux["baz"]);
    }

    [Fact]
    public void TestList()
    {
        var module = Env.TestRichReturn();
        var result = Assert.Single(module.FooBarBazList());
        Assert.Equal(1, result.Foo);
        Assert.Equal("hello", result.Bar);
        Assert.Equal([1L, 2L, 3L], result.Baz);
        Assert.Equal((42L, "world"), result.Qux);
        Assert.Equal(3, result.Quux.Count);
        Assert.Equal(1, result.Quux["foo"]);
        Assert.Equal(2, result.Quux["bar"]);
        Assert.Equal(3, result.Quux["baz"]);
    }

    [Fact]
    public void TestDict()
    {
        var module = Env.TestRichReturn();
        var (key, value) = Assert.Single(module.FooBarBazDict("key"));
        Assert.Equal("key", key);
        Assert.Equal(1, value.Foo);
        Assert.Equal("hello", value.Bar);
        Assert.Equal([1L, 2L, 3L], value.Baz);
        Assert.Equal((42L, "world"), value.Qux);
        Assert.Equal(3, value.Quux.Count);
        Assert.Equal(1, value.Quux["foo"]);
        Assert.Equal(2, value.Quux["bar"]);
        Assert.Equal(3, value.Quux["baz"]);
    }
}
