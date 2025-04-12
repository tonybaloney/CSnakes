using System.Collections;
using CSnakes.Runtime.Python.Internals;

namespace CSnakes.Runtime.Python;

public sealed class GeneratorIterator<TYield, TSend, TReturn>(PyObject coroutine) :
    GeneratorIterator<TYield, TSend, TReturn,
                      PyObjectImporters.Runtime<TYield>,
                      PyObjectImporters.Runtime<TReturn>>(coroutine);

public class GeneratorIterator<TYield, TSend, TReturn, TYieldImporter, TReturnImporter>(PyObject generator) :
    IGeneratorIterator<TYield, TSend, TReturn>
    where TYieldImporter : IPyObjectImporter<TYield>
    where TReturnImporter : IPyObjectImporter<TReturn>
{
    private bool _disposed = false;
    private readonly PyObject generator = generator;
    private readonly PyObject nextPyFunction = generator.GetAttr("__next__");
    private readonly PyObject closePyFunction = generator.GetAttr("close");
    private readonly PyObject sendPyFunction = generator.GetAttr("send");

    private TYield current = default!;
    private TReturn @return = default!;

    public TYield Current => current;
    public TReturn Return => @return;

    object IEnumerator.Current => Current!;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            generator.Dispose();
            nextPyFunction.Dispose();
            closePyFunction.Call().Dispose();
            closePyFunction.Dispose();
            sendPyFunction.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator<TYield> GetEnumerator() => this;

    public bool MoveNext() => Send(PyObject.None);

    public void Reset() => throw new NotSupportedException();

    public bool Send(TSend value)
    {
        using PyObject sendValue = PyObject.From(value);
        return Send(sendValue);
    }

    private bool Send(PyObject value)
    {
        try
        {
            using PyObject result = sendPyFunction.Call(value);
            current = result.ImportAs<TYield, TYieldImporter>();
            return true;
        }
        catch (PythonInvocationException ex)
        {
            if (ex.InnerException is PythonStopIterationException stopIteration)
            {
                using var @return = stopIteration.TakeValue();
                this.@return = @return.ImportAs<TReturn, TReturnImporter>();
                return false;
            }

            throw;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this;
}
