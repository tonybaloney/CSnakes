using CSnakes.Runtime.CPython;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

[RequiresDynamicCode(DynamicCodeMessages.CallsMakeGenericType)]
internal sealed class PyList<T>(PyObject listObject) :
    PyList<T, PyObjectImporters.Runtime<T>>(listObject);

internal class PyList<T, TImporter>(PyObject listObject) :
    IReadOnlyList<T>, IDisposable, ICloneable
    where TImporter : IPyObjectImporter<T>
{
    // If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
    private readonly Dictionary<long, T> _convertedItems = [];

    public T this[int index]
    {
        get
        {
            if (_convertedItems.TryGetValue(index, out T? cachedValue))
            {
                return cachedValue;
            }

            using (GIL.Acquire())
            {
                using PyObject value = PyObject.Create(CPythonAPI.PySequence_GetItem(listObject, index));
                var result = TImporter.BareImport(value);
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

    public IEnumerator<T> GetEnumerator()
    {
        // TODO: If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
        using (GIL.Acquire())
        {
            using var items = new PyEnumerable<T, TImporter>(listObject.Clone());
            return items.GetEnumerator();
        }
    }

    PyObject ICloneable.Clone() => listObject.Clone();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
