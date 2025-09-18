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
        Task<PyObject> task;

        using (GIL.Acquire())
            task = CPythonAPI.GetDefaultEventLoop().RunCoroutineAsync(coroutine, cancellationToken);

        var result = await task.ConfigureAwait(false);

        try
        {
            using (GIL.Acquire())
                return current = TYieldImporter.BareImport(result);
        }
        catch (PythonInvocationException ex)
        {
            if (ex.InnerException is PythonStopIterationException stopIteration)
            {
                using var @return = stopIteration.TakeValue();
                this.@return = @return.ImportAs<TReturn, TReturnImporter>();

                // Coroutine has been issued stop iteration, not the same as an async function returning a value.
                throw stopIteration;
            }

            throw;
        }
    }
}
