using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyList<TItem> : IReadOnlyList<TItem>, IDisposable
{
    private readonly PyObject _listObject;

    public PyList(PyObject listObject) => _listObject = listObject;

    public TItem this[int index]
    {
        get
        {
            using (GIL.Acquire())
            {
                using PyObject value = PyObject.Create(CPythonAPI.PySequence_GetItem(_listObject, index));
                return value.As<TItem>();
            }
        }
    }

    public int Count
    {
        get
        {
            using (GIL.Acquire())
            {
                return (int)CPythonAPI.PySequence_Size(_listObject);
            }
        }
    }

    public void Dispose()
    {
        _listObject.Dispose();
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        using (GIL.Acquire())
        {
            return new PyEnumerable<TItem>(_listObject);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
