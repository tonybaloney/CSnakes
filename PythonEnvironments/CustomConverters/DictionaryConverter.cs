using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonEnvironments.CustomConverters;

internal class DictionaryConverter<TKey, TValue> : IPyObjectEncoder, IPyObjectDecoder
{
    public bool CanDecode(PyType objectType, Type targetType)
    {
        throw new NotImplementedException();
    }

    public bool CanEncode(Type type)
    {
        //if (type.IsGenericType && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)))
        if (type.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(type))
        {
            return true;
        }

        return false;
    }

    public bool TryDecode<T>(PyObject pyObj, out T? value)
    {
        throw new NotImplementedException();
    }

    public PyObject TryEncode(object value)
    {
        var dict = (IReadOnlyDictionary<TKey, TValue>)value;

        var pyDict = new PyDict();
        foreach (var kvp in dict)
        {
            pyDict.SetItem(kvp.Key.ToPython(), kvp.Value.ToPython());
        }

        return pyDict;
    }
}
