﻿using Python.Runtime;
using System;
using System.Collections.Generic;

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
