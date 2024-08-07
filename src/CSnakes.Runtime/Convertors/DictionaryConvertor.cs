using CSnakes.Runtime.Convertors;
using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections.ObjectModel;

namespace CSnakes.Runtime.Converters;

public sealed class DictionaryConvertor<TKey, TValue> : IPythonConvertor
    where TKey : notnull
{
    public bool CanDecode(PyObject objectType, Type targetType) =>
        targetType.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(targetType);

    public bool CanEncode(Type type) =>
        type.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(type);

    public bool TryEncode(object value, out PyObject? result)
    {
        var pyDict = CPythonAPI.PyDict_New();

        var dict = (IReadOnlyDictionary<TKey, TValue>)value;

        foreach (var kvp in dict)
        {
            int hresult = CPythonAPI.PyDict_SetItem(pyDict, kvp.Key.ToPython().DangerousGetHandle(), kvp.Value.ToPython().DangerousGetHandle());
            if (hresult == -1)
            {
                result = null;
                // TODO: Forward exception
                return false;
            }
        }

        result = new PyObject(pyDict);
        return true;
    }

    public bool TryDecode(PyObject pyObj, out object? result)
    {
        if (!CPythonAPI.IsPyDict(pyObj.DangerousGetHandle()))
        {
            result = default!;
            return false;
        }

        var items = CPythonAPI.PyDict_Items(pyObj.DangerousGetHandle()); // Newref
        var dict = new Dictionary<TKey, TValue>();
        nint itemsLength = CPythonAPI.PyList_Size(items);
        for (nint i = 0; i < itemsLength; i++)
        {
            var item = new PyObject(CPythonAPI.PyList_GetItem(items, i)); // Borrowed
            var t = item.As<Tuple<TKey, TValue>>();
            dict.Add(t.Item1, t.Item2);
        }

        result = new ReadOnlyDictionary<TKey, TValue>(dict);
        return true;
    }
}
