using System.Collections;

namespace CSnakes.Runtime.Python;
public class GeneratorIterator<TYield, TSend, TReturn>(PyObject generator) : IGeneratorIterator<TYield, TSend, TReturn>
{
    private readonly PyObject generator = generator;
    private readonly PyObject nextPyFunction = generator.GetAttr("__next__");
    private readonly PyObject closePyFunction = generator.GetAttr("close");
    private readonly PyObject sendPyFunction = generator.GetAttr("send");

    private TYield current = default!;

    public TYield Current => current;

    object IEnumerator.Current => Current!;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            generator.Dispose();
            nextPyFunction.Dispose();
            closePyFunction.Call().Dispose();
            closePyFunction.Dispose();
            sendPyFunction.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator<TYield> GetEnumerator() => this;


    public bool MoveNext()
    {
        try
        {
            using PyObject result = nextPyFunction.Call();
            current = result.As<TYield>();
            return true;
        }
        catch (PythonInvocationException pyO) when (pyO.PythonExceptionType == "StopIteration")
        {
            return true;
        }

    }

    public void Reset() => throw new NotImplementedException();


    public TYield Send(TSend value)
    {
        try
        {
            using PyObject sendValue = PyObject.From<TSend>(value) !;
            using PyObject result = sendPyFunction.Call(sendValue);
            current = result.As<TYield>();
            return current;
        }
        catch (PythonInvocationException pyO) when (pyO.PythonExceptionType == "StopIteration")
        {
            return true;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this;

}
