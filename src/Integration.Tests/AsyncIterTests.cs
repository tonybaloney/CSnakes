using System.Collections.Generic;
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

}
