using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        IEnumerable<PyObject> pyValues = pyObj.AsEnumerable<PyObject>();

        // We have to convert the Python values to CLR values, as if we just tried As<object>() it would
        // not parse the Python type to a CLR type, only to a new Python type.
        Type[] types = typeof(T).GetGenericArguments();
        object?[] clrValues = pyValues.Select((p, i) => p.AsManagedObject(types[i])).ToArray();

        ConstructorInfo ctor = typeof(T).GetConstructors().First(c => c.GetParameters().Count() == clrValues.Length);
        value = (T)ctor.Invoke(clrValues);

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