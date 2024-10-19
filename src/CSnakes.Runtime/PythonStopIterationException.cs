using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;

public sealed class PythonStopIterationException : PythonRuntimeException
{
    private PyObject? value;

    public PythonStopIterationException(PyObject exception, PyObject? traceback) : base(exception, traceback)
    {
        const string attr = "value";
        this.value = exception.HasAttr(attr) && exception.GetAttr(attr) is var value ? value : PyObject.None;
    }

    /// <summary>
    /// Yields the ownership of the <see
    /// href="https://docs.python.org/3/library/exceptions.html#StopIteration.value"><c>value</c></see>
    /// attribute of the <see
    /// href="https://docs.python.org/3/library/exceptions.html#StopIteration"><c>StopIteration</c></see>
    /// exception by returning it to the caller.
    /// </summary>
    /// <exception cref="InvalidOperationException">The value was taken already.</exception>
    /// <remarks>
    /// It's the responsibility of the caller to dispose of the returned value.
    /// </remarks>
    public PyObject TakeValue()
    {
        if (this.value is null)
            throw new InvalidOperationException();

        var value = this.value;
        this.value = null;
        return value;
    }
}
