using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        var tasks =
            from _ in Enumerable.Range(0, 10)
            select mod.TestCoroutine();
        _ = await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task MultipleCoroutineCallsIsParallelThatGetCancelled()
    {
        var mod = Env.TestCoroutines();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        var tasks =
            from _ in Enumerable.Range(0, 10)
            select mod.TestCoroutine(seconds: 5, cancellationTokenSource.Token);

        foreach (var task in tasks)
        {
            async Task Act() => await task.WaitAsync(TimeSpan.FromSeconds(10));
            var ex = await Assert.ThrowsAsync<TaskCanceledException>(Act);
            Assert.Equal(cancellationTokenSource.Token, ex.CancellationToken);
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }
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
