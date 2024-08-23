using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToList(PyObject pyObject, Type destinationType)
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
            list.Add(ConvertTo(item, genericArgument));
        }

        return list;
    }

    private object? ConvertToListFromSequence(PyObject pyObject, Type destinationType)
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
            list.Add(ConvertTo(item, genericArgument));
        }

        return list;
    }

    private PyObject ConvertFromList(IEnumerable e)
    {
        List<PyObject> pyObjects = [];

        foreach (object? item in e)
        {
            pyObjects.Add(ToPython(item));
        }

        return Pack.CreateList(pyObjects);
    }
}