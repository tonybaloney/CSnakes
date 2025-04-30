namespace CSnakes.Runtime.Python;
internal sealed class AsyncIterator<T, TImporter>(PyObject pyObject, CancellationToken cancellationToken) :
    IAsyncEnumerator<T>
    where TImporter : IPyObjectImporter<ICoroutine<T, PyObject, PyObject>>
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
            using var nextResult = anextFunction.Call();
            ICoroutine<T, PyObject, PyObject> coroutine;
            using (GIL.Acquire())
                coroutine = TImporter.BareImport(nextResult);
            var task = coroutine.AsTask(cancellationToken);

            T item;

            try
            {
                item = await task.ConfigureAwait(false);
            }
            catch (PythonInvocationException ex) when (ex.PythonExceptionType is "StopAsyncIteration")
            {
                yield break;
            }

            yield return item;
        }
    }
}
