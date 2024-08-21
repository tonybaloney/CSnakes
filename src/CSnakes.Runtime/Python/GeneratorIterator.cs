using System.Collections;

namespace CSnakes.Runtime.Python;
public class GeneratorIterator<TYield, TSend, TReturn> : IGeneratorIterator<TYield, TSend, TReturn>
{
    private readonly PyObject generator;
    private readonly PyObject nextPyFunction;
    private readonly PyObject closePyFunction;
    private readonly PyObject sendPyFunction;

    private TYield current = default!;

    public GeneratorIterator(PyObject generator)
    {
        this.generator = generator;
        nextPyFunction = generator.GetAttr("__next__");
        closePyFunction = generator.GetAttr("close");
        sendPyFunction = generator.GetAttr("send");
    }

    public TYield Current => current;

    object IEnumerator.Current => Current!;

    public void Dispose()
    {
        generator.Dispose();
        nextPyFunction.Dispose();
        closePyFunction.Call().Dispose();
        closePyFunction.Dispose();
        sendPyFunction.Dispose();
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
        catch (PythonInvocationException pyO)
        {
            if (pyO.PythonExceptionType == "StopIteration")
            {
                return false;
            }
            throw;
        }
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public TYield Send(TSend value)
    {
        try
        {
            using PyObject sendValue = PyObject.From<TSend>(value) !;
            using PyObject result = sendPyFunction.Call(sendValue);
            current = result.As<TYield>();
            return current;
        }
        catch (PythonInvocationException pyO)
        {
            if (pyO.PythonExceptionType == "StopIteration")
            {
                throw new ArgumentOutOfRangeException("Generator is exhausted");
            }
            throw;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }
}
