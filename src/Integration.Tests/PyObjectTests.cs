using CSnakes.Runtime.Python;
using System.Linq;

namespace Integration.Tests;
public class PyObjectTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory]
    [InlineData("[1, 2, 3, 4, 5]")]
    [InlineData("(1, 2, 3, 4, 5)")]
    [InlineData("range(1, 6)")]
    public void AsEnumerable_SourceCanBeReiterated(string expression)
    {
        using var range = Env.ExecuteExpression(expression);
        var expected = new[] { 1, 2, 3, 4, 5 };
        var xs = range.AsEnumerable<int>();
        Assert.Equal(expected, xs.ToArray());   // First iteration
        Assert.Equal(15, xs.Sum());             // Second iteration
    }

    [Fact]
    public void AsEnumerable_IteratorsAreIndependent()
    {
        using var range = Env.ExecuteExpression("range(1, 6)");
        var xs = range.AsEnumerable<int>();
        var result = xs.Zip(xs, xs.Skip(1));
        Assert.Equal([(1, 1, 2),
                      (2, 2, 3),
                      (3, 3, 4),
                      (4, 4, 5)],
                     result);
    }

    [Fact]
    public void AsEnumerable_Clones()
    {
        using var range = Env.ExecuteExpression("range(1, 6)");
        var xs = range.AsEnumerable<int>();
        range.Dispose();
        using var e = xs.GetEnumerator();
        Assert.True(e.MoveNext());
    }

    [Fact]
    public void AsEnumerable_IteratorReleasesGilBetweenYields()
    {
        using var range = Env.ExecuteExpression("range(1, 6)");
        foreach (var _ in range.AsEnumerable<int>())
            Assert.False(GIL.IsAcquired);
    }

    [Fact]
    public void AsEnumerable_IteratorFailsOnFirstIterationForIncompatibleObject()
    {
        using var range = Env.ExecuteExpression("42");
        var xs = range.AsEnumerable<int>();

        void Act() => xs.GetEnumerator().Dispose();

        var pyInvocationException = Assert.Throws<PythonInvocationException>(Act);
        var pyRuntimeException = Assert.IsType<PythonRuntimeException>(pyInvocationException.InnerException);
        Assert.Equal("'int' object is not iterable", pyRuntimeException.Message);
    }
}
