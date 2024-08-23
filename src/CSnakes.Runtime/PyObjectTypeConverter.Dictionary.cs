using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Collections.ObjectModel;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToDictionary(PyObject pyObject, Type destinationType, bool useMappingProtocol = false)
    {
        using PyObject items = useMappingProtocol ? 
            PyObject.Create(CPythonAPI.PyMapping_Items(pyObject)) : 
            PyObject.Create(CPythonAPI.PyDict_Items(pyObject));

        Type item1Type = destinationType.GetGenericArguments()[0];
        Type item2Type = destinationType.GetGenericArguments()[1];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type dictType = typeof(Dictionary<,>).MakeGenericType(item1Type, item2Type);
            Type returnType = typeof(ReadOnlyDictionary<,>).MakeGenericType(item1Type, item2Type);

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
            using PyObject item = PyObject.Create(CPythonAPI.PyList_GetItem(items, i));

            using PyObject item1 = PyObject.Create(CPythonAPI.PyTuple_GetItem(item, 0));
            using PyObject item2 = PyObject.Create(CPythonAPI.PyTuple_GetItem(item, 1));

            object? convertedItem1 = ConvertTo(item1, item1Type);
            object? convertedItem2 = ConvertTo(item2, item2Type);

            dict.Add(convertedItem1!, convertedItem2);
        }

        return typeInfo.ReturnTypeConstructor.Invoke([dict]);
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