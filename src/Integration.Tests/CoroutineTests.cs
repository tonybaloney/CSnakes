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

    [Fact]
    public async Task CoroutineRaisesException()
    {
        var mod = Env.TestCoroutines();
        var task = mod.TestCoroutineRaisesException();
        var exception = await Assert.ThrowsAsync<PythonInvocationException>(async () => await task);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("This is a Python exception", exception.InnerException.Message);
        Assert.Equal("ValueError", exception.PythonExceptionType);
    }
}
