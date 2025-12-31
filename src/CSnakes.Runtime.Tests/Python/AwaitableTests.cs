using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class AwaitableTests : RuntimeTestBase
{
    private PyObject AsyncIoSleep()
    {
        var locals = new Dictionary<string, PyObject>();
        var globals = new Dictionary<string, PyObject>();

        using var result = env.Execute(locals: locals, globals: globals, code: """
            import asyncio
            a = asyncio.sleep(0)

            """);

        return locals["a"];
    }

    public class PyObjectWaitAsync : AwaitableTests
    {
        [Fact]
        public void WhenNotAwaitable_Throws()
        {
            using var result = env.ExecuteExpression("42");

            void Act() => Awaitable.WaitAsync(result, TestContext.Current.CancellationToken);
            var ex = Assert.Throws<ArgumentException>(Act);

            Assert.Equal("obj", ex.ParamName);
        }

        [Fact]
        public async Task DoesNotDisposeWhenWaitIsDoneObject()
        {
            using var awaitable = AsyncIoSleep();
            _ = await Awaitable.WaitAsync(awaitable, TestContext.Current.CancellationToken);

            Assert.False(awaitable.IsClosed);
        }
    }

    public enum DisposalTiming { AwaitTaskThenDispose, DisposeThenAwaitTask }

    public interface IWaitAsync<TSelf, in T>
        where TSelf : IWaitAsync<TSelf, T>
        where T : IAwaitable
    {
        /// <remarks>
        /// Implementations should invoke the <c>WaitAsync</c> method on the given <paramref
        /// name="awaitable"/> and return the task directly without using <see langword="await"/> on
        /// it.
        /// </remarks>
        static abstract Task<PyObject> Invoke(T awaitable, CancellationToken cancellationToken);
    }

    public abstract class WaitAsync<T, TWaitAsync> : AwaitableTests
        where T : IAwaitable
        where TWaitAsync : IWaitAsync<TWaitAsync, T>
    {
        [Fact]
        public async Task DoesNotDisposeWhenWaitIsDone()
        {
            using var awaitableObject = AsyncIoSleep();
            using var awaitable = awaitableObject.As<T>();

            (await TWaitAsync.Invoke(awaitable, TestContext.Current.CancellationToken)).Dispose();

            Assert.False(awaitableObject.IsClosed);
        }

        /// <summary>
        /// Test that <c>WaitAsync</c> does not care about the timing of the awaitable's disposal
        /// because the implementation should take a clone while awaiting.
        /// </summary>
        [Theory]
        [InlineData(DisposalTiming.AwaitTaskThenDispose)]
        [InlineData(DisposalTiming.DisposeThenAwaitTask)]
        public async Task ObliviousToDisposalTiming(DisposalTiming disposalTiming)
        {
            const string code = """
                class MyAwaitable:
                    def __init__(self, log: list[str]):
                        self.log = log

                    def __await__(self):
                        self.log.append(f"Awaiting {self}")
                        yield
                        self.log.append(f"{self} completed")

                    def __del__(self):
                        self.log.append(f"Deleting {self}")

                """;

            var locals = new Dictionary<string, PyObject>();
            var globals = new Dictionary<string, PyObject>();
            using var result = env.Execute(code, locals, globals);

            (string BeforeAwait, string AfterAwait, string Deletion) expectedLog;
            using var log = PyObject.From(Array.Empty<string>());

            Task<PyObject> task;
            using (PyObject type = locals["MyAwaitable"], awaitableObject = type.Call(log))
            {
                expectedLog = ($"Awaiting {awaitableObject}",
                               $"{awaitableObject} completed",
                               $"Deleting {awaitableObject}");

                using var awaitable = awaitableObject.As<T>();
                task = TWaitAsync.Invoke(awaitable, TestContext.Current.CancellationToken);

                if (disposalTiming == DisposalTiming.AwaitTaskThenDispose)
                {
                    (await task).Dispose();
                    Assert.Equal([expectedLog.BeforeAwait, expectedLog.AfterAwait], log.AsEnumerable<string>());
                }
            }

            if (disposalTiming == DisposalTiming.DisposeThenAwaitTask)
                (await task).Dispose();

            Assert.Equal([expectedLog.BeforeAwait, expectedLog.AfterAwait, expectedLog.Deletion],
                         log.AsEnumerable<string>());
        }
    }

    public class NonGenericWaitAsync : WaitAsync<IAwaitable, NonGenericWaitAsync>, IWaitAsync<NonGenericWaitAsync, IAwaitable>
    {
        static Task<PyObject> IWaitAsync<NonGenericWaitAsync, IAwaitable>.Invoke(IAwaitable awaitable,
                                                                                 CancellationToken cancellationToken) =>
            awaitable.WaitAsync(cancellationToken);
    }

    public class GenericWaitAsync : WaitAsync<IAwaitable<PyObject>, GenericWaitAsync>, IWaitAsync<GenericWaitAsync, IAwaitable<PyObject>>
    {
        static Task<PyObject> IWaitAsync<GenericWaitAsync, IAwaitable<PyObject>>.Invoke(IAwaitable<PyObject> awaitable,
                                                                                        CancellationToken cancellationToken) =>
            awaitable.WaitAsync(cancellationToken);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WithDisposeParam(bool dispose)
        {
            using var awaitableObject = AsyncIoSleep();
            using var awaitable = awaitableObject.As<IAwaitable<PyObject>>();

            (await awaitable.WaitAsync(dispose: dispose, TestContext.Current.CancellationToken)).Dispose();

            Assert.Equal(dispose, awaitable.DangerousInternalReference.IsClosed);
        }
    }
}
