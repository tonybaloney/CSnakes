using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

internal sealed class PyAsyncEnumerable<T, TImporter>(PyObject pyAsyncIterable) : IAsyncEnumerable<T>, IDisposable
    where TImporter : IPyObjectImporter<T>
{
    public void Dispose() => pyAsyncIterable.Dispose();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken) =>
        Iterator(pyAsyncIterable.GetAIter(), cancellationToken);

    private static async IAsyncEnumerator<T> Iterator(PyObject pyIterator, CancellationToken cancellationToken)
    {
        using var _ = pyIterator;
        using var anextFunction = pyIterator.GetAttr("__anext__");

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

                item = obj.ImportAs<T, TImporter>();
            }
            finally
            {
                obj?.Dispose();
            }

            yield return item;
        }
    }
}
