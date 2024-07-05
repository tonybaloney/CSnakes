using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonEnvironments.CustomConverters;

public sealed class ListConverter<TKey> : IPyObjectEncoder, IPyObjectDecoder
{
    public bool CanDecode(PyType objectType, Type targetType)
    {
        if (targetType.IsGenericType && typeof(IEnumerable<TKey>).IsAssignableFrom(targetType))
        {
            return true;
        }
        return false;
    }

    public bool CanEncode(Type type)
    {
        if (type.IsGenericType && typeof(IEnumerable<TKey>).IsAssignableFrom(type))
        {
            return true;
        }

        return false;
    }

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        value = (T)(object)pyObj.AsEnumerable<TKey>().ToList();
        return true;
    }

    public PyObject TryEncode(object value)
    {
        var list = (IEnumerable<TKey>)value;

        var pyList = new PyList();
        foreach (var item in list)
        {
            pyList.Append(item.ToPython());
        }

        return pyList;
    }
}
