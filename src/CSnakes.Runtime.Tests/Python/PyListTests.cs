using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class PyListTests : RuntimeTestBase
{
    [Fact]
    public void TestGetEnumeratorFunctionsDespiteEarlyListDisposal()
    {
        var list = PyObject.From(new[] { 42 }).As<IReadOnlyList<int>>();

        using var e = list.GetEnumerator();
        ((IDisposable)list).Dispose();

        Assert.True(e.MoveNext());
        Assert.Equal(42, e.Current);
        Assert.False(e.MoveNext());
    }
}
