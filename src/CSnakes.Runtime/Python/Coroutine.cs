using CSnakes.Runtime.CPython;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

[RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
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
                return this.current = TYieldImporter.BareImport(result);
        }
        catch (PythonInvocationException ex)
        {
            if (ex.InnerException is PythonStopIterationException stopIteration)
            {
                using var @return = stopIteration.TakeValue();
                this.@return = @return.ImportAs<TReturn, TReturnImporter>();

                // Coroutine has finished
                // TODO: define behavior for this case
                return default;
            }

            throw;
        }
    }
}
