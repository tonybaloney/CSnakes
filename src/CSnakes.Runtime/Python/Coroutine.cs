using CSnakes.Runtime.CPython;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

[RequiresDynamicCode(DynamicCodeMessages.CallsMakeGenericType)]
internal sealed class Coroutine<T>(PyObject coroutine) :
    Coroutine<T, PyObjectImporters.Runtime<T>>(coroutine);

internal class Coroutine<T, TImporter>(PyObject coroutine) : ICoroutine<T>
    where TImporter : IPyObjectImporter<T>
{
    public async Task<T> AsTask(CancellationToken cancellationToken = default)
    {
        Task<PyObject> task;

        using (GIL.Acquire())
            task = CPythonAPI.GetDefaultEventLoop().RunCoroutineAsync(coroutine, cancellationToken);

        var result = await task.ConfigureAwait(false);

        using (GIL.Acquire())
            return TImporter.BareImport(result);
    }
}
