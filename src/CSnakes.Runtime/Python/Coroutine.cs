using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python.Internals;

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
                        current = TYieldImporter.Import(result);
                    }
                    return current;
                }
                catch (PythonInvocationException ex)
                {
                    if (ex.InnerException is PythonStopIterationException stopIteration)
                    {
                        using var @return = stopIteration.TakeValue();
                        this.@return = TReturnImporter.Import(@return);

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
