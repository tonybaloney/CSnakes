using CSnakes.Runtime.Python;
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
        long result = await mod.TestCoroutine(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task MultipleCoroutineCalls()
    {
        var mod = Env.TestCoroutines();
        var tasks = new List<Task<long>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(mod.TestCoroutine(cancellationToken: TestContext.Current.CancellationToken));
        }
        var r = await Task.WhenAll(tasks);
        Assert.All(r, x => Assert.Equal(5, x));
    }

    [Fact]
    public async Task SequentialCoroutinesWithCompletedCancellation()
    {
        var mod = Env.TestCoroutines();

        using CancellationTokenSource cts = new();

        Task RunAsync() => mod.TestCoroutine(seconds: 0, cancellationToken: cts.Token)
                              .WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        // First call should complete successfully.

        await RunAsync();

        cts.Cancel();

        // Second call should be cancelled immediately (implicitly tests that
        // the event loop is still functional).

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => RunAsync());
        Assert.Equal(cts.Token, ex.CancellationToken);
    }

    [Fact]
    public async Task SequentialCoroutinesWithErrorCancellation()
    {
        var mod = Env.TestCoroutines();

        using CancellationTokenSource cts = new();

        Task RunAsync() => mod.TestCoroutineRaisesException(cancellationToken: cts.Token)
                              .WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    
        // First call should throw an error.

        await Assert.ThrowsAsync<PythonInvocationException>(() => RunAsync());

        cts.Cancel();

        // Second call should be cancelled immediately (implicitly tests that
        // the event loop is still functional).

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => RunAsync());
        Assert.Equal(cts.Token, ex.CancellationToken);
    }

    [Fact]
    public async Task CoroutineThatSelfCancels()
    {
        var mod = Env.TestCoroutines();
        var task = mod.TestCoroutineSelfCanceling(TestContext.Current.CancellationToken);
        var ex = await Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.Equal(CancellationToken.None, ex.CancellationToken);
        Assert.Equal(TaskStatus.Canceled, task.Status);
    }

    [Fact]
    public async Task MultipleCoroutineCallsIsParallel()
    {
        var mod = Env.TestCoroutines();
        var tasks =
            from _ in Enumerable.Range(0, 10)
            select mod.TestCoroutine(cancellationToken: TestContext.Current.CancellationToken);
        _ = await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
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
            async Task Act() => await task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            var ex = await Assert.ThrowsAsync<TaskCanceledException>(Act);
            Assert.Equal(cancellationTokenSource.Token, ex.CancellationToken);
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }
    }

    [Fact]
    public async Task CoroutineRaisesException()
    {
        var mod = Env.TestCoroutines();
        var task = mod.TestCoroutineRaisesException(TestContext.Current.CancellationToken);
        var exception = await Assert.ThrowsAsync<PythonInvocationException>(async () => await task);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("This is a Python exception", exception.InnerException.Message);
        Assert.Equal("ValueError", exception.PythonExceptionType);
    }

    [Fact]
    public async Task CoroutineReturnsNothing()
    {
        var mod = Env.TestCoroutines();
        await mod.TestCoroutineReturnsNothing(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task BareCoroutine()
    {
        var mod = Env.TestCoroutines();
        long result = await mod.TestCoroutineBare(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(5, result);
    }

    [Fact]
    public void GeneratorBasedCoroutineIsNotSupported()
    {
        var mod = Env.TestCoroutines();
        using var coro = mod.TestGeneratorBasedCoroutine();
        void Act() => _ = coro.As<ICoroutine<PyObject>>();
        Assert.Throws<InvalidCastException>(Act);
    }
}
