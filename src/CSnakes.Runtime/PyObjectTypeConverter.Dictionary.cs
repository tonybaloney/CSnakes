using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Collections.ObjectModel;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object ConvertToDictionary(PyObject pyObject, Type destinationType, bool useMappingProtocol = false)
    {
        using PyObject items = useMappingProtocol ? 
            PyObject.Create(CPythonAPI.PyMapping_Items(pyObject)) : 
            PyObject.Create(CPythonAPI.PyDict_Items(pyObject));

        Type keyType = destinationType.GetGenericArguments()[0];
        Type valueType = destinationType.GetGenericArguments()[1];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            Type returnType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);

            typeInfo = new(returnType.GetConstructor([dictType])!, dictType.GetConstructor([typeof(int)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        nint itemsLength = CPythonAPI.PyList_Size(items);
        IDictionary dict = (IDictionary)typeInfo.TransientTypeConstructor!.Invoke([(int)itemsLength]);

        for (nint i = 0; i < itemsLength; i++)
        {
            // TODO: We make 3 heap allocations per item here.
            // 1. The item, which could be inlined as it's only used to call PyTuple_GetItem.
            // 2. The key, which we need to recursively convert -- although if this is a string, which it mostly is, then we _could_ avoid this.
            // 3. The value, which we need to recursively convert.
            nint kvpTuple = CPythonAPI.PyList_GetItem(items, i);

            // Optimize keys as string because this is the most common case.
            if (keyType == typeof(string))
            {
                nint itemKey = CPythonAPI.PyTuple_GetItemWithNewRefRaw(kvpTuple, 0);
                using PyObject value = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRefRaw(kvpTuple, 1));

                string? keyAsString = CPythonAPI.PyUnicode_AsUTF8Raw(itemKey);
                if (keyAsString is null)
                {
                    CPythonAPI.Py_DecRefRaw(itemKey);
                    PyObject.ThrowPythonExceptionAsClrException();
                }
                object? convertedValue = value.As(valueType);

                dict.Add(keyAsString!, convertedValue);
                CPythonAPI.Py_DecRefRaw(itemKey);
            } else
            {
                using PyObject key = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRefRaw(kvpTuple, 0));
                using PyObject value = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRefRaw(kvpTuple, 1));

                object? convertedKey = key.As(keyType);
                object? convertedValue = value.As(valueType);

                dict.Add(convertedKey!, convertedValue);
            }
            CPythonAPI.Py_DecRefRaw(kvpTuple);
        }

        return typeInfo.ReturnTypeConstructor.Invoke([dict]);
    }

    internal IReadOnlyDictionary<TKey, TValue> ConvertToDictionary<TKey, TValue>(PyObject pyObject) where TKey : notnull
    {
        using PyObject items = PyObject.Create(CPythonAPI.PyMapping_Items(pyObject));

        var dict = new Dictionary<TKey, TValue>();
        nint itemsLength = CPythonAPI.PyList_Size(items);
        for (nint i = 0; i < itemsLength; i++)
        {
            nint kvpTuple = CPythonAPI.PyList_GetItem(items, i);
            using PyObject key = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRefRaw(kvpTuple, 0));
            using PyObject value = PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRefRaw(kvpTuple, 1));

            TKey convertedKey = key.As<TKey>();
            TValue convertedValue = value.As<TValue>();

            dict.Add(convertedKey, convertedValue);
            CPythonAPI.Py_DecRefRaw(kvpTuple);
        }

        return dict;
    }

    private PyObject ConvertFromDictionary(IDictionary dictionary)
    {
        int len = dictionary.Keys.Count;
        PyObject[] keys = new PyObject[len];
        PyObject[] values = new PyObject[len];

        int i = 0;
        foreach (DictionaryEntry kvp in dictionary)
        {
            keys[i] = ToPython(kvp.Key);
            values[i] = ToPython(kvp.Value);
            i++;
        }

        return Pack.CreateDictionary(keys, values);
    }
}