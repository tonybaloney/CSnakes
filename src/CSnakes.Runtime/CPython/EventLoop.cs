using CSnakes.Runtime.Python;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSnakes.Runtime.CPython;
internal sealed class EventLoop : IDisposable
{
    private bool disposed;
    private readonly PyObject loop = CPythonAPI.NewEventLoop();
    private Methods methods;
    private Task runForeverTask;
    private readonly ConcurrentQueue<Request> requestQueue = new();
    private readonly PyObject futureLoopStopFunction;

    private abstract class Request : IDisposable
    {
        public virtual void Dispose() { }
    }

    private sealed class ScheduleRequest : Request
    {
        public static ScheduleRequest Create(PyObject awaitable, CancellationToken cancellationToken)
        {
            var clone = awaitable.Clone();
            try
            {
                return new ScheduleRequest(clone, cancellationToken);
            }
            catch
            {
                clone.Dispose();
                throw;
            }
        }

        private ScheduleRequest(PyObject awaitable, CancellationToken cancellationToken)
        {
            Awaitable = awaitable;
            CancellationToken = cancellationToken;
        }

        public PyObject Awaitable { get; }
        public CancellationToken CancellationToken { get; }
        public TaskCompletionSource<TaskCompletionSource<PyObject>> CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override void Dispose() => Awaitable.Dispose();
    }

    private sealed class CancelRequest(Future future, CancellationToken cancellationToken) : Request
    {
        public Future Future { get; } = future;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }

    private sealed class StopRequest : Request
    {
        public static readonly StopRequest Instance = new();

        private StopRequest() { }
    }

    private sealed class Future(PyObject pyFuture) : IDisposable
    {
        private readonly PyObject pyFuture = pyFuture;
        private PyObject? doneMethod;
        private CancellationToken cancellationToken;

        public enum LifecycleChangeKind { Canceling, Completed }
        public event Action<Future, LifecycleChangeKind>? LifecycleChange;

        public TaskCompletionSource<PyObject> CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <remarks>
        /// Since <c>done</c> is expected to be called many times, cache the the
        /// method object to avoid the overhead of calling <see
        /// cref="PyObject.GetAttr"/> each time.
        /// </remarks>
        private PyObject DoneMethod => this.doneMethod ??= pyFuture.GetAttr("done");

        private bool HasCompleted => this.pyFuture.IsClosed;

        public void Dispose()
        {
            this.doneMethod?.Dispose();
            this.pyFuture.Dispose();
        }

        public void Cancel(CancellationToken cancellationToken = default)
        {
            if (HasCompleted)
                return;

            LifecycleChange?.Invoke(this, LifecycleChangeKind.Canceling);

            using var cancelMethod = pyFuture.GetAttr("cancel");
            cancelMethod.Call().Dispose();
            this.cancellationToken = cancellationToken;
        }

        public bool Conclude()
        {
            if (ProcessConclusion() == TaskStatus.Running)
                return false;

            LifecycleChange?.Invoke(this, LifecycleChangeKind.Completed);

            Dispose();
            return true;
        }

        private TaskStatus ProcessConclusion()
        {
            if (HasCompleted)
                return CompletionSource.Task.Status;

            // If the future is not done yet, return without doing anything.

            if (!DoneMethod.Call())
                return TaskStatus.Running;

            // If the Python future is cancelled, set the corresponding .NET task to cancelled.

            using (var cancelledMethod = this.pyFuture.GetAttr("cancelled"))
            {
                if (cancelledMethod.Call())
                {
                    CompletionSource.SetCanceled(this.cancellationToken);
                    return TaskStatus.Canceled;
                }
            }

            // If the Python future raised an exception, set the corresponding .NET task to faulted.

            using (var exceptionMethod = this.pyFuture.GetAttr("exception"))
            {
                PyObject? exception = exceptionMethod.Call();
                try
                {
                    if (!exception.IsNone())
                    {
                        using var type = exception.GetPythonType();
                        string name = type.GetAttr("__name__").ImportAs<string, PyObjectImporters.String>();
                        // TODO We are effectively losing the traceback here so copy the traceback or somehow attach it to "PythonInvocationException"
                        // https://github.com/tonybaloney/CSnakes/pull/438#discussion_r2068321787
                        CompletionSource.SetException(new PythonInvocationException(name, exception, null));
                        exception = null;
                        return TaskStatus.Faulted;
                    }
                }
                finally
                {
                    exception?.Dispose();
                }
            }

            // If the Python future is finished, set the result in the corresponding .NET task.

            using var resultFunction = this.pyFuture.GetAttr("result");
            PyObject? result = resultFunction.Call();
            try
            {
                CompletionSource.SetResult(result);
                result = null; // yield ownership
                return TaskStatus.RanToCompletion;
            }
            finally
            {
                result?.Dispose();
            }
        }
    }

    public static EventLoop RunNewForever() => new();

    private EventLoop()
    {
        this.methods = new Methods(this.loop);
        using var vars = PyObject.Create(CPythonAPI.PyDict_New());
        this.futureLoopStopFunction = CPythonAPI.PyRun_String("lambda fut: fut.get_loop().stop()", CPythonAPI.InputType.Py_eval_input, vars, vars);
        this.runForeverTask = Task.Run(RunForever);
    }

    public void Dispose()
    {
        if (this.disposed)
            return;

        try
        {
            Enqueue(StopRequest.Instance);
            this.runForeverTask.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            _ = this.methods.Close.Call();
        }
        catch (Exception ex)
        {
            Debug.Fail(ex.Message, ex.ToString());
        }

        this.methods.Dispose();
        this.loop.Dispose();
        this.futureLoopStopFunction.Dispose();
        this.disposed = true;
    }

    public async Task<PyObject> RunAsync(PyObject awaitable, CancellationToken cancellationToken)
    {
        using var request = ScheduleRequest.Create(awaitable, cancellationToken);
        Enqueue(request);
        var taskCompletionSource = await request.CompletionSource.Task.ConfigureAwait(false);
        return await taskCompletionSource.Task.ConfigureAwait(false);
    }

    private void Enqueue(Request request)
    {
        this.requestQueue.Enqueue(request);
        _ = this.methods.CallSoonThreadSafe.Call(this.methods.Stop);
    }

    enum RunState
    {
        Running,
        Stopping,
    }

    private void RunForever()
    {
        var state = RunState.Running;
        var futures = new List<Future>();

        do
        {
            // Run the event loop forever. The event loop gets stopped and the call returns when
            // either of the following happens:
            //
            // - The cancellation token for the run is triggered.
            // - A request to schedule a new awaitable as a future is received.
            // - A future is done.

            _ = this.methods.RunForever.Call();

            while (requestQueue.TryDequeue(out var poppedRequest))
            {
                using (poppedRequest)
                {
                    switch (poppedRequest, state)
                    {
                        case (ScheduleRequest request, RunState.Stopping):
                        {
                            // Cancel any request to schedule an awaitable if the event loop is
                            // stopping.

                            request.CompletionSource.SetCanceled();
                            break;
                        }
                        case (ScheduleRequest { CancellationToken.IsCancellationRequested: true } request, _):
                        {
                            // Cancel any request to schedule an awaitable if the task cancellation
                            // token is triggered.

                            request.CompletionSource.SetCanceled(request.CancellationToken);
                            break;
                        }
                        case (ScheduleRequest request, RunState.Running):
                        {
                            // Normalize an awaitable to a future and add a done callback to it that
                            // will stop the event loop when the future is done.

                            PyObject pyFuture;
                            try
                            {
                                pyFuture = CPythonAPI.EnsureFuture(request.Awaitable, this.loop);
                            }
                            catch (Exception ex)
                            {
                                // If the future could not even be created, set the exception.
                                // This should almost never happen unless there is an error in our logic!

                                request.CompletionSource.SetException(ex);
                                break;
                            }

                            IDisposable disposable = pyFuture;
                            CancellationTokenRegistration cancellationRegistration = default;

                            try
                            {
                                var future = new Future(pyFuture);
                                disposable = future; // yield ownership

                                using (var addDoneCallbackMethod = pyFuture.GetAttr("add_done_callback"))
                                    _ = addDoneCallbackMethod.Call(this.futureLoopStopFunction);

                                if (request.CancellationToken is { CanBeCanceled: true } cancellationToken)
                                {
                                    cancellationRegistration =
                                        cancellationToken.Register(() => Enqueue(new CancelRequest(future, cancellationToken)),
                                                                   useSynchronizationContext: false);

                                    future.LifecycleChange += (_, kind) =>
                                    {
                                        if (kind is Future.LifecycleChangeKind.Canceling or Future.LifecycleChangeKind.Completed)
                                            cancellationRegistration.Dispose();
                                    };
                                }

                                // Create a "TaskCompletionSource" to represent the future on the
                                // .NET side and add it to the list of futures.

                                futures.Add(future);

                                // Signal that the future was successfully scheduled.

                                request.CompletionSource.SetResult(future.CompletionSource);
                            }
                            catch (Exception ex)
                            {
                                // If the future could not be setup, set the exception.

                                request.CompletionSource.SetException(ex);

                                // Clean up any resources allocated for the future.

                                cancellationRegistration.Dispose();
                                disposable.Dispose();
                            }
                            break;
                        }
                        case (CancelRequest request, _):
                        {
                            request.Future.Cancel(request.CancellationToken);
                            break;
                        }
                        case (StopRequest, RunState.Running):
                        {
                            state = RunState.Stopping;
                            foreach (var future in futures)
                                future.Cancel(CancellationToken.None);
                            break;
                        }
                    }
                }
            }

            _ = futures.RemoveAll(t => t.Conclude());
        }
        while (state is RunState.Running || futures.Count > 0);
    }

    private struct Methods(PyObject loop) : IDisposable
    {
        private PyObject? callSoonThreadSafe;
        private PyObject? runForever;
        private PyObject? stop;
        private PyObject? close;

        public PyObject CallSoonThreadSafe => this.callSoonThreadSafe ??= loop.GetAttr("call_soon_threadsafe");
        public PyObject RunForever => this.runForever ??= loop.GetAttr("run_forever");
        public PyObject Stop => this.stop ??= loop.GetAttr("stop");
        public PyObject Close => this.close ??= loop.GetAttr("close");

        public void Dispose()
        {
            this.callSoonThreadSafe?.Dispose();
            this.callSoonThreadSafe = null;
            this.runForever?.Dispose();
            this.runForever = null;
            this.stop?.Dispose();
            this.stop = null;
            this.close?.Dispose();
            this.close = null;
        }
    }
}
