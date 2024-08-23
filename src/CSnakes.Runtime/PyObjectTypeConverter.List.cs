using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToList(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        Type genericArgument = destinationType.GetGenericArguments()[0];

        nint listSize = CPythonAPI.PySequence_Size(pyObject);
        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type listType = typeof(List<>).MakeGenericType(genericArgument);
            typeInfo = new(listType.GetConstructor([typeof(int)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        IList list = (IList)typeInfo.ReturnTypeConstructor.Invoke([(int)listSize]);

        for (var i = 0; i < listSize; i++)
        {
            using PyObject item = PyObject.Create(CPythonAPI.PyList_GetItem(pyObject, i));
            list.Add(ConvertTo(context, culture, item, genericArgument));
        }

        return list;
    }

    private object? ConvertToListFromSequence(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        Type genericArgument = destinationType.GetGenericArguments()[0];

        nint listSize = CPythonAPI.PySequence_Size(pyObject);
        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type listType = typeof(List<>).MakeGenericType(genericArgument);
            typeInfo = new(listType.GetConstructor([typeof(int)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        IList list = (IList)typeInfo.ReturnTypeConstructor.Invoke([(int)listSize]);

        for (var i = 0; i < listSize; i++)
        {
            using PyObject item = PyObject.Create(CPythonAPI.PySequence_GetItem(pyObject, i));
            list.Add(ConvertTo(context, culture, item, genericArgument));
        }

        return list;
    }

    private PyObject ConvertFromList(ITypeDescriptorContext? context, CultureInfo? culture, IEnumerable e)
    {
        PyObject pyList = PyObject.Create(CPythonAPI.PyList_New(0));

        foreach (var item in e)
        {
            PyObject converted = ToPython(item, context, culture);
            int result = CPythonAPI.PyList_Append(pyList, converted);
            if (result == -1)
            {
                PyObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyList;
    }
}