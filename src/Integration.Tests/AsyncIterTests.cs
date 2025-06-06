using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integration.Tests;
public class AsyncIterTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task TestAsyncIter_Basic()
    {
        var mod = Env.TestAsyncIter();

        await using var asyncIter = mod.TestBasicAsyncIter().AsAsyncEnumerator<int>();
        var result = new List<int>();

        while (await asyncIter.MoveNextAsync())
            result.Add(asyncIter.Current);

        Assert.Equal([1, 2, 3, 4, 5], result);
    }

    [Fact]
    public async Task TestAsyncIterable_Basic()
    {
        var mod = Env.TestAsyncIter();
        var stream = mod.TestBasicAsyncIterable().AsAsyncEnumerable<int>();

        var list = await stream.ToListAsync();
        var sum = await stream.SumAsync();

        Assert.Equal([1, 2, 3, 4, 5], list);
        Assert.Equal(15, sum);
    }
}
