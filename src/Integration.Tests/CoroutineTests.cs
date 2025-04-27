using CSnakes.Runtime.Python;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
    public async Task MultipleCoroutineCallsIsParallel()
    {
        var mod = Env.TestCoroutines();
        var tasks = new List<Task<PyObject>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(mod.TestCoroutineReturnsNothing());
        }
        // Check this takes less than 10 seconds
        var start = Stopwatch.StartNew();
        var r = await Task.WhenAll(tasks);
        start.Stop();
        Assert.True(start.ElapsedMilliseconds < 10000);
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

    [Fact]
    public async Task CoroutineReturnsNothing()
    {
        var mod = Env.TestCoroutines();
        var result = await mod.TestCoroutineReturnsNothing();
        Assert.True(result.IsNone());
    }
}
