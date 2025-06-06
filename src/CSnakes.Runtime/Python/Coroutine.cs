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
    public async Task<TReturn> AsTask(CancellationToken cancellationToken = default)
    {
        Task<PyObject> task;

        using (GIL.Acquire())
            task = CPythonAPI.GetDefaultEventLoop().RunCoroutineAsync(coroutine, cancellationToken);

        var result = await task.ConfigureAwait(false);

        using (GIL.Acquire())
            return TReturnImporter.BareImport(result);
    }
}
