using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;

public sealed class PythonStopIterationException : PythonRuntimeException
{
    private PyObject? value;

    public PythonStopIterationException(PyObject? exception, PyObject? traceback) : base(exception, traceback)
    {
        // `PyObject` uses the `PyErr_Fetch` API to get the exception and traceback. Unfortunately,
        // it looks like there was a regression with Python version 3.12. Prior to 3.12, the
        // `PyErr_Fetch` API would return the exception's value in `p_value`:
        // https://github.com/python/cpython/blob/0c47759eee3e170e04a5dae82f12f6b375ae78f7/Python/errors.c#L430
        //
        // In version 3.12+, it was changed to return the exception object:
        // https://github.com/python/cpython/blob/v3.12.0/Python/errors.c#L503
        //
        // So to get at the value of the `StopIteration` exception, check if the object type name is
        // indeed `StopIteration` and only then get the `value` attribute. Otherwise, assume that
        // the object is the exception's value!

        if (exception is { } valueOrError)
        {
            using var type = valueOrError.GetPythonType();
            using var typeName = type.GetAttr("__name__");
            this.value = PyObjectImporters.String.Import(typeName) == "StopIteration"
                       ? valueOrError.GetAttr("value")
                       : valueOrError;
        }
        else
        {
            this.value = PyObject.None;
        }
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
