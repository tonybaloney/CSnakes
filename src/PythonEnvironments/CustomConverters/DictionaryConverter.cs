using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PythonEnvironments.CustomConverters;

public sealed class DictionaryConverter<TKey, TValue> : IPyObjectEncoder, IPyObjectDecoder
{
    public bool CanDecode(PyType objectType, Type targetType)
    {
        if (targetType.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(targetType))
        {
            return true;
        }
        return false;
    }

    public bool CanEncode(Type type)
    {
        if (type.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(type))
        {
            return true;
        }

        return false;
    }

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        var pyDict = new PyDict(pyObj);

        //var dict = pyDict.Items()
        //    .Select(item => item.As<Tuple<TKey, TValue>>())
        //    .ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);

        var items = pyDict.Items();

        var dict = new Dictionary<TKey, TValue>();

        foreach (var item in items)
        {
            var t = item.As<Tuple<TKey, TValue>>();
            dict.Add(t.Item1, t.Item2);
        }

        value = (T)(object)new ReadOnlyDictionary<TKey, TValue>(dict);
        return true;
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
