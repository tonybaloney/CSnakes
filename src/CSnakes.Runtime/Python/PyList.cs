using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyList<TItem> : IReadOnlyList<TItem>, IDisposable
{
    private readonly PyObject _listObject;

    // If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
    private readonly Dictionary<long, TItem> _convertedItems = new();

    public PyList(PyObject listObject) => _listObject = listObject;

    public TItem this[int index]
    {
        get
        {
            if (_convertedItems.TryGetValue(index, out TItem? cachedValue))
            {
                return cachedValue;
            }

            using (GIL.Acquire())
            {
                using PyObject value = PyObject.Create(CPythonAPI.PySequence_GetItem(_listObject, index));
                TItem result = value.As<TItem>();
                _convertedItems[index] = result;
                return result;
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

    public void Dispose() => _listObject.Dispose();

    public IEnumerator<TItem> GetEnumerator()
    {
        // TODO: If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
        using (GIL.Acquire())
        {
            return new PyEnumerable<TItem>(_listObject);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
