using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public class Coroutine<TYield, TSend, TReturn>(PyObject coroutine) : ICoroutine<TYield, TSend, TReturn>
{
    private TYield current = default!;
    private TReturn @return = default!;

    public TYield Current => current;
    public TReturn Return => @return;

    public Task<TYield?> AsTask(CancellationToken? cancellationToken = null)
    {
        return Task.Run(
            () =>
            {
                try
                {
                    using (GIL.Acquire())
                    {
                        using PyObject result = CPythonAPI.GetEventLoop().RunTaskUntilComplete(coroutine, cancellationToken);
                        current = result.As<TYield>();
                    }
                    return current;
                }
                catch (PythonInvocationException ex)
                {
                    if (ex.InnerException is PythonStopIterationException stopIteration)
                    {
                        using var @return = stopIteration.TakeValue();
                        this.@return = @return.As<TReturn>();

                        // Coroutine has finished
                        // TODO: define behavior for this case
                        return default;
                    }

                    throw;
                }
            }
        );
    }
}
