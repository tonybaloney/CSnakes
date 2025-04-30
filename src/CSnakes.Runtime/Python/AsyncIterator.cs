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
        try
        {
            using PyObject nextResult = _anextPyFunction.Call();
            ICoroutine<TYield, PyObject, PyObject> coroutine;
            using (GIL.Acquire())
                coroutine = TImporter.BareImport(nextResult);
            current = await coroutine.AsTask() ?? throw new InvalidOperationException("Async iterator returned null");
            return true;
        }
        catch (PythonInvocationException pyExc)
        {
            // If the exception is a StopAsyncIteration, we return false
            // Otherwise, we rethrow the exception
            if (pyExc.PythonExceptionType == "StopAsyncIteration")
            {
                return false;
            }
            throw;
        }
    }
}
