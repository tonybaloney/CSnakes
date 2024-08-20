using System.Collections;

namespace CSnakes.Runtime.Python;
public class GeneratorIterator<TYield, TSend, TReturn> : IEnumerator<TYield>, IEnumerable<TYield>, IGenerator<TYield, TSend, TReturn>
{
    private readonly PyObject generator;
    private readonly PyObject next_func;
    private readonly PyObject close_func;
    private readonly PyObject send_func;

    private TYield current = default;

    public GeneratorIterator(PyObject generator)
    {
        this.generator = generator;
        next_func = generator.GetAttr("__next__");
        close_func = generator.GetAttr("close");
        send_func = generator.GetAttr("send");
    }

    public TYield Current => current;

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        generator.Dispose();
        next_func.Dispose();
    }

    public IEnumerator<TYield> GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        try
        {
            using PyObject result = next_func.Call();
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
            using PyObject result = send_func.Call(sendValue);
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

    public void Close()
    {
        using PyObject close = close_func.Call();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }
}
