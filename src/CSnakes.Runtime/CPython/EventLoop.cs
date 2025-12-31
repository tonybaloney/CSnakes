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
    private readonly PyObject taskLoopStopFunction;

    private abstract class Request : IDisposable
    {
        public virtual void Dispose() { }
    }

    private sealed class ScheduleRequest : Request
    {
        public static ScheduleRequest Create(PyObject coroutine, CancellationToken cancellationToken)
        {
            var clone = coroutine.Clone();
            try
            {
                return new ScheduleRequest(clone, cancellationToken);
            }
            catch
            {
                coroutine.Dispose();
                throw;
            }
        }

        private ScheduleRequest(PyObject coroutine, CancellationToken cancellationToken)
        {
            Coroutine = coroutine;
            CancellationToken = cancellationToken;
        }

        public PyObject Coroutine { get; }
        public CancellationToken CancellationToken { get; }
        public TaskCompletionSource<TaskCompletionSource<PyObject>> CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override void Dispose() => Coroutine.Dispose();
    }

    private sealed class CancelRequest(CoroutineTask coroTask, CancellationToken cancellationToken) : Request
    {
        public CoroutineTask Task { get; } = coroTask;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }

    private sealed class StopRequest : Request
    {
        public static readonly StopRequest Instance = new();

        private StopRequest() { }
    }

    private sealed class CoroutineTask(PyObject pyTask) : IDisposable
    {
        private readonly PyObject pyTask = pyTask;
        private PyObject? doneMethod;
        private CancellationToken cancellationToken;

        public enum LifecycleEvent { CancellationRequested, Completed }
        public event Action<CoroutineTask, LifecycleEvent>? LifecycleChanged;

        public TaskCompletionSource<PyObject> CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <remarks>
        /// Since <c>done</c> is expected to be called many times, cache the the
        /// method object to avoid the overhead of calling <see
        /// cref="PyObject.GetAttr"/> each time.
        /// </remarks>
        private PyObject DoneMethod => this.doneMethod ??= pyTask.GetAttr("done");

        private bool HasCompleted => this.pyTask.IsClosed;

        public void Dispose()
        {
            this.doneMethod?.Dispose();
            this.pyTask.Dispose();
        }

        public void Cancel(CancellationToken cancellationToken = default)
        {
            if (HasCompleted)
                return;

            LifecycleChanged?.Invoke(this, LifecycleEvent.CancellationRequested);

            using var cancelMethod = pyTask.GetAttr("cancel");
            cancelMethod.Call().Dispose();
            this.cancellationToken = cancellationToken;
        }

        public bool Conclude()
        {
            if (ProcessConclusion() == TaskStatus.Running)
                return false;

            LifecycleChanged?.Invoke(this, LifecycleEvent.Completed);

            Dispose();
            return true;
        }

        private TaskStatus ProcessConclusion()
        {
            if (HasCompleted)
                return CompletionSource.Task.Status;

            // If the task is not done yet, return without doing anything.

            if (!DoneMethod.Call())
                return TaskStatus.Running;

            // If the Python task is cancelled, set the corresponding .NET task to cancelled.

            using (var cancelledMethod = this.pyTask.GetAttr("cancelled"))
            {
                if (cancelledMethod.Call())
                {
                    CompletionSource.SetCanceled(this.cancellationToken);
                    return TaskStatus.Canceled;
                }
            }

            // If the Python task raised an exception, set the corresponding .NET task to faulted.

            using (var exceptionMethod = this.pyTask.GetAttr("exception"))
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

            // If the Python task is finished, set the result in the corresponding .NET task.

            using var resultFunction = this.pyTask.GetAttr("result");
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
        this.taskLoopStopFunction = CPythonAPI.PyRun_String("lambda task: task.get_loop().stop()", CPythonAPI.InputType.Py_eval_input, vars, vars);
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
        this.taskLoopStopFunction.Dispose();
        this.disposed = true;
    }

    public async Task<PyObject> RunCoroutineAsync(PyObject coroutine, CancellationToken cancellationToken)
    {
        using var request = ScheduleRequest.Create(coroutine, cancellationToken);
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
        var coroTasks = new List<CoroutineTask>();

        do
        {
            // Run the event loop forever. The event loop gets stopped and the call returns when
            // either of the following happens:
            //
            // - The cancellation token for the run is triggered.
            // - A request to schedule a new coroutine as a task is received.
            // - A running task is done.

            _ = this.methods.RunForever.Call();

            while (requestQueue.TryDequeue(out var poppedRequest))
            {
                using (poppedRequest)
                {
                    switch (poppedRequest, state)
                    {
                        case (ScheduleRequest request, RunState.Stopping):
                        {
                            // Cancel any request to schedule a coroutine if the event loop is
                            // stopping.

                            request.CompletionSource.SetCanceled();
                            break;
                        }
                        case (ScheduleRequest { CancellationToken.IsCancellationRequested: true } request, _):
                        {
                            // Cancel any request to schedule a coroutine if the task cancellation
                            // token is triggered.

                            request.CompletionSource.SetCanceled(request.CancellationToken);
                            break;
                        }
                        case (ScheduleRequest request, RunState.Running):
                        {
                            // Create a task and add a done callback to it that will stop
                            // the event loop when the task is done.

                            var pyTask = this.methods.CreateTask.Call(request.Coroutine);
                            IDisposable disposable = pyTask;
                            CancellationTokenRegistration cancellationRegistration = default;

                            try
                            {
                                var coroTask = new CoroutineTask(pyTask);
                                disposable = coroTask; // yield ownership

                                using (var addDoneCallbackMethod = pyTask.GetAttr("add_done_callback"))
                                    _ = addDoneCallbackMethod.Call(this.taskLoopStopFunction);

                                if (request.CancellationToken is { CanBeCanceled: true } cancellationToken)
                                {
                                    cancellationRegistration = cancellationToken.Register(
                                        () => Enqueue(new CancelRequest(coroTask, cancellationToken)),
                                        useSynchronizationContext: false);

                                    coroTask.LifecycleChanged += (_, state) =>
                                    {
                                        if (state == CoroutineTask.LifecycleEvent.CancellationRequested || state == CoroutineTask.LifecycleEvent.Completed)
                                            cancellationRegistration.Dispose();
                                    };
                                }

                                // Create a "TaskCompletionSource" to represent the task on the
                                // .NET side and add it to the list of tasks.

                                coroTasks.Add(coroTask);

                                // Signal that the task was successfully scheduled.

                                request.CompletionSource.SetResult(coroTask.CompletionSource);
                            }
                            catch (Exception ex)
                            {
                                // If the task could not be scheduled, set the exception.

                                request.CompletionSource.SetException(ex);

                                // Clean up any resources allocated for the task.

                                cancellationRegistration.Dispose();
                                disposable.Dispose();
                            }
                            break;
                        }
                        case (CancelRequest request, _):
                        {
                            request.Task.Cancel(request.CancellationToken);
                            break;
                        }
                        case (StopRequest, RunState.Running):
                        {
                            state = RunState.Stopping;
                            foreach (var task in coroTasks)
                                task.Cancel(CancellationToken.None);
                            break;
                        }
                    }
                }
            }

            _ = coroTasks.RemoveAll(t => t.Conclude());
        }
        while (state is RunState.Running || coroTasks.Count > 0);
    }

    private struct Methods(PyObject loop) : IDisposable
    {
        private PyObject? createTask;
        private PyObject? callSoonThreadSafe;
        private PyObject? runForever;
        private PyObject? stop;
        private PyObject? close;

        public PyObject CreateTask => this.createTask ??= loop.GetAttr("create_task");
        public PyObject CallSoonThreadSafe => this.callSoonThreadSafe ??= loop.GetAttr("call_soon_threadsafe");
        public PyObject RunForever => this.runForever ??= loop.GetAttr("run_forever");
        public PyObject Stop => this.stop ??= loop.GetAttr("stop");
        public PyObject Close => this.close ??= loop.GetAttr("close");

        public void Dispose()
        {
            this.createTask?.Dispose();
            this.createTask = null;
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
