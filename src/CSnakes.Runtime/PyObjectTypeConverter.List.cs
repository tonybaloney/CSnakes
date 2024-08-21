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
        Type listType = typeof(List<>).MakeGenericType(genericArgument);

        IList list = (IList)Activator.CreateInstance(listType)!;
        for (var i = 0; i < CPythonAPI.PyList_Size(pyObject); i++)
        {
            using PyObject item = new(CPythonAPI.PyList_GetItem(pyObject, i));
            list.Add(ConvertTo(context, culture, item, genericArgument));
        }

        return list;
    }

    private object? ConvertToListFromSequence(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        Type genericArgument = destinationType.GetGenericArguments()[0];
        Type listType = typeof(List<>).MakeGenericType(genericArgument);

        IList list = (IList)Activator.CreateInstance(listType)!;
        for (var i = 0; i < CPythonAPI.PySequence_Size(pyObject); i++)
        {
            using PyObject item = new(CPythonAPI.PySequence_GetItem(pyObject, i));
            list.Add(ConvertTo(context, culture, item, genericArgument));
        }

        return list;
    }

    private PyObject ConvertFromList(ITypeDescriptorContext? context, CultureInfo? culture, IEnumerable e)
    {
        PyObject pyList = new(CPythonAPI.PyList_New(0));

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