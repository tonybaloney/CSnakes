using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PythonEnvironments.CustomConverters;

public class TupleConverter : IPyObjectEncoder, IPyObjectDecoder
{
    public bool CanDecode(PyType objectType, Type targetType)
    {
        return typeof(ITuple).IsAssignableFrom(targetType);
    }

    public bool CanEncode(Type type)
    {
        return typeof(ITuple).IsAssignableFrom(type);
    }

    public bool TryDecode<T>(PyObject pyObj, out T? value)
    {
        var values = pyObj.AsEnumerable<object>().ToArray();

        var creates = typeof(Tuple).GetMethods().Where(m => m.Name == "Create");

        var create = creates.First(m => m.GetParameters().Count() == values.Length);

        var method = create.MakeGenericMethod(values.Select(v => v.GetType()).ToArray());

        value = (T)method.Invoke(null, [.. values]);
        return true;
    }

    public PyObject TryEncode(object value)
    {
        var t = (ITuple)value;

        List<PyObject> pyObjects = [];

        for (var i = 0; i < t.Length; i++)
        {
            pyObjects.Add(t[i].ToPython());
        }

        return new PyTuple([.. pyObjects]);
    }
}