using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToDictionary(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture, bool useMappingProtocol = false)
    {
        using PyObject items = useMappingProtocol ? new(CPythonAPI.PyMapping_Items(pyObject)) : new(CPythonAPI.PyDict_Items(pyObject));
        Type item1Type = destinationType.GetGenericArguments()[0];
        Type item2Type = destinationType.GetGenericArguments()[1];
        Type dictType = typeof(Dictionary<,>).MakeGenericType(item1Type, item2Type);
        IDictionary dict = (IDictionary)Activator.CreateInstance(dictType)!;
        nint itemsLength = CPythonAPI.PyList_Size(items);

        for (nint i = 0; i < itemsLength; i++)
        {
            using PyObject item = new(CPythonAPI.PyList_GetItem(items, i));

            using PyObject item1 = new(CPythonAPI.PyTuple_GetItem(item, 0));
            using PyObject item2 = new(CPythonAPI.PyTuple_GetItem(item, 1));

            object? convertedItem1 = ConvertTo(context, culture, item1, item1Type);
            object? convertedItem2 = ConvertTo(context, culture, item2, item2Type);

            dict.Add(convertedItem1!, convertedItem2);
        }

        Type returnType = typeof(ReadOnlyDictionary<,>).MakeGenericType(item1Type, item2Type);
        return Activator.CreateInstance(returnType, dict);
    }

    private PyObject ConvertFromDictionary(ITypeDescriptorContext? context, CultureInfo? culture, IDictionary dictionary)
    {
        PyObject pyDict = new(CPythonAPI.PyDict_New());

        foreach (DictionaryEntry kvp in dictionary)
        {
            int result = CPythonAPI.PyDict_SetItem(pyDict, ToPython(kvp.Key, context, culture), ToPython(kvp.Value, context, culture));
            if (result == -1)
            {
                PyObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyDict;
    }
}