using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;
internal sealed class AsyncIterator<T, TImporter>(PyObject pyObject, CancellationToken cancellationToken) :
    IAsyncEnumerator<T>
    where TImporter : IPyObjectImporter<T>
{
    private readonly IAsyncEnumerator<T> enumerator = Iterator(pyObject, cancellationToken);

    public T Current => enumerator.Current;

    public ValueTask<bool> MoveNextAsync() => enumerator.MoveNextAsync();

    public ValueTask DisposeAsync()
    {
        pyObject.Dispose();
        return ValueTask.CompletedTask;
    }

    private static async IAsyncEnumerator<T> Iterator(PyObject pyObject, CancellationToken cancellationToken)
    {
        using var anextFunction = pyObject.GetAttr("__anext__");

        while (true)
        {
            Task<PyObject> task;
            using var coroutine = anextFunction.Call();
            using (GIL.Acquire())
                task = CPythonAPI.GetDefaultEventLoop().RunCoroutineAsync(coroutine.Clone(), cancellationToken);

            PyObject? obj = null;
            T item;

            try
            {
                try
                {
                    obj = await task.ConfigureAwait(false);
                }
                catch (PythonInvocationException ex) when (ex.PythonExceptionType is "StopAsyncIteration")
                {
                    yield break;
                }

                using (GIL.Acquire())
                    item = TImporter.BareImport(obj);
            }
            finally
            {
                obj?.Dispose();
            }

            yield return item;
        }
    }
}
