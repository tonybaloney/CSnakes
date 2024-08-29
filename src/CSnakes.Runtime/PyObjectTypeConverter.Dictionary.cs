using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object ConvertToDictionary(PyObject pyObject, Type destinationType, bool useMappingProtocol = false)
    {
        Type keyType = destinationType.GetGenericArguments()[0];
        Type valueType = destinationType.GetGenericArguments()[1];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type dictType = typeof(PyDictionary<,>).MakeGenericType(keyType, valueType);

            typeInfo = new(dictType.GetConstructor([typeof(PyObject)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }

    internal static IReadOnlyDictionary<TKey, TValue> ConvertToDictionary<TKey, TValue>(PyObject pyObject) where TKey : notnull
    { 
        return new PyDictionary<TKey, TValue>(pyObject);
    }

    internal static PyObject ConvertFromDictionary(IDictionary dictionary)
    {
        int len = dictionary.Keys.Count;
        PyObject[] keys = new PyObject[len];
        PyObject[] values = new PyObject[len];

        int i = 0;
        foreach (DictionaryEntry kvp in dictionary)
        {
            keys[i] = PyObject.From(kvp.Key);
            values[i] = PyObject.From(kvp.Value);
            i++;
        }

        return Pack.CreateDictionary(keys, values);
    }
}