using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyKeyValuePairEnumerable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerator<KeyValuePair<TKey, TValue>>
{
    private readonly PyObject _pyIterator;
    private KeyValuePair<TKey, TValue> current = default!;

    internal PyKeyValuePairEnumerable(PyObject pyObject)
    {
        using (GIL.Acquire())
        {
            _pyIterator = pyObject.GetIter();
        }
    }

    public KeyValuePair<TKey, TValue> Current => current;

    object IEnumerator.Current => current!;

    public void Dispose() => _pyIterator.Dispose();

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this;

    public bool MoveNext()
    {
        using (GIL.Acquire())
        {
            nint result = CPythonAPI.PyIter_Next(_pyIterator);
            if (result == IntPtr.Zero && CPythonAPI.PyErr_Occurred())
            {
                throw PyObject.ThrowPythonExceptionAsClrException();
            }

            if (result == IntPtr.Zero)
            {
                return false;
            }

            using PyObject pyObject = PyObject.Create(result);
            current = pyObject.As<TKey, TValue>();
            return true;
        }
    }

    public void Reset() => throw new NotSupportedException("Python iterators cannot be reset");

    IEnumerator IEnumerable.GetEnumerator() => this;
}
