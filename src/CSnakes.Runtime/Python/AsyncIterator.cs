namespace CSnakes.Runtime.Python;

internal class AsyncIterator<TYield, TImporter> : IAsyncIterator<TYield>
    where TImporter : IPyObjectImporter<ICoroutine<TYield, PyObject, PyObject>>
{
    private readonly PyObject _pyAsyncIterator;
    private TYield current = default!;
    private readonly PyObject _anextPyFunction;

    internal AsyncIterator(PyObject pyObject)
    {
        using (GIL.Acquire())
        {
            _pyAsyncIterator = pyObject.GetAIter();
            _anextPyFunction = pyObject.GetAttr("__anext__");
        }
    }

    public TYield Current => current;

    public ValueTask DisposeAsync()
    {
        _pyAsyncIterator.Dispose();
        return new ValueTask();
    }

    public IAsyncEnumerator<TYield> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

    public async ValueTask<bool> MoveNextAsync()
    {
        using PyObject nextResult = _anextPyFunction.Call();
        ICoroutine<TYield, PyObject, PyObject> coroutine;
        using (GIL.Acquire())
            coroutine = TImporter.BareImport(nextResult);
        var task = coroutine.AsTask() ?? throw new InvalidOperationException("Async iterator returned null");

        try
        {
            current = await task.ConfigureAwait(false);
            return true;
        }
        catch (PythonInvocationException ex) when (ex.PythonExceptionType is "StopAsyncIteration")
        {
            return false;
        }
    }
}
