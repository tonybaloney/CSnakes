using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PyList<TItem>(PyObject listObject, Func<PyObject, TItem>? converter) :
    IReadOnlyList<TItem>, IDisposable, ICloneable
{
    // If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
    private readonly Dictionary<long, TItem> _convertedItems = [];

    public PyList(PyObject listObject) : this(listObject, null) { }

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
                using PyObject value = PyObject.Create(CPythonAPI.PySequence_GetItem(listObject, index));
                var result = converter is { } someConverter
                           ? someConverter(value)
                           : value.As<TItem>();
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
                return (int)CPythonAPI.PySequence_Size(listObject);
            }
        }
    }

    public void Dispose() => listObject.Dispose();

    public IEnumerator<TItem> GetEnumerator()
    {
        // TODO: If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
        using (GIL.Acquire())
        {
            return new PyEnumerable<TItem>(listObject);
        }
    }

    PyObject ICloneable.Clone() => listObject.Clone();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
