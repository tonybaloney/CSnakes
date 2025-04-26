using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public sealed class Coroutine<TYield, TSend, TReturn>(PyObject coroutine) :
    Coroutine<TYield, TSend, TReturn,
              PyObjectImporters.Runtime<TYield>,
              PyObjectImporters.Runtime<TReturn>>(coroutine);

public class Coroutine<TYield, TSend, TReturn, TYieldImporter, TReturnImporter>(PyObject coroutine) :
    ICoroutine<TYield, TSend, TReturn>
    where TYieldImporter : IPyObjectImporter<TYield>
    where TReturnImporter : IPyObjectImporter<TReturn>
{
    private TYield current = default!;
    private TReturn @return = default!;

    public TYield Current => current;
    public TReturn Return => @return;


    public async Task<TYield> AsTask(CancellationToken cancellationToken = default)
    {
        var taskCompletionSource = new TaskCompletionSource<TYield>();

        var runTask = Task.Run(
            () =>
            {
                try
                {
                    using (GIL.Acquire())
                    {
                        using PyObject result = CPythonAPI.GetEventLoop().RunTaskUntilComplete(coroutine, cancellationToken);
                        current = TYieldImporter.BareImport(result);
                    }
                    taskCompletionSource.SetResult(current);
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case { InnerException: PythonStopIterationException stopIteration }:
                        {
                            using var @return = stopIteration.TakeValue();
                            this.@return = @return.ImportAs<TReturn, TReturnImporter>();

                            // Coroutine has finished
                            // TODO: define behavior for this case
                            return;
                        }
                        case PythonInvocationException { PythonExceptionType: "CancelledError" }:
                            taskCompletionSource.SetCanceled(cancellationToken);
                            return;
                    }

                    taskCompletionSource.SetException(ex);
                }
            }
        );

        await runTask.ConfigureAwait(false);

        return await taskCompletionSource.Task.ConfigureAwait(false);
    }
}
