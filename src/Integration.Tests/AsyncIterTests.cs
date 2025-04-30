using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integration.Tests;
public class AsyncIterTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task TestAsyncIter_Basic()
    {
        var mod = Env.TestAsyncIter();

        var asyncIter = mod.TestBasicAsyncIter().AsAsyncIterator<int>();
        var result = new List<int>();

        await foreach (var item in asyncIter)
        {
            result.Add(item);
        }

        Assert.Equal([1, 2, 3, 4, 5], result);
    }

}
