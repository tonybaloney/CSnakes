#pragma warning disable PRTEXP001

using Integration.Tests;
using System.Collections.Immutable;
using static CSnakes.Linq.PyObjectReader;

namespace CSnakes.Linq.Tests;
public class QueryTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    public sealed class FooBarBaz : IPyObjectReadable<FooBarBaz>
    {
        public long Foo { get; private init;  }
        public string? Bar { get; private init; }
        public required ImmutableArray<long> Baz { get; init; }
        public (long, string) Qux { get; private init; }
        public required ImmutableDictionary<string, long> Quux { get; init; }

        public static IPyObjectReader<FooBarBaz> Reader =>
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
        var module = Env.TestQuery();
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
        var module = Env.TestQuery();
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
}
