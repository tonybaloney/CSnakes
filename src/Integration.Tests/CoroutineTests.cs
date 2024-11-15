namespace Integration.Tests;
public class CoroutineTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task BasicCoroutine()
    {
        var mod = Env.TestCoroutines();
        long result = await mod.TestCoroutine();
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task MultipleCoroutineCalls()
    {
        var mod = Env.TestCoroutines();
        var tasks = new List<Task<long>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(mod.TestCoroutine());
        }
        var r = await Task.WhenAll(tasks);
        Assert.All(r, x => Assert.Equal(5, x));
    }
}
