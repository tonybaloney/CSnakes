using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private ICollection ConvertToList(PyObject pyObject, Type destinationType)
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

    private ICollection ConvertToListFromSequence(PyObject pyObject, Type destinationType)
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

    internal IReadOnlyCollection<TItem> ConvertToCollection<TItem>(PyObject pyObject)
    {
        nint listSize = CPythonAPI.PySequence_Size(pyObject);
        var list = new List<TItem>((int)listSize);
        for (var i = 0; i < listSize; i++)
        {
            using PyObject item = PyObject.Create(CPythonAPI.PySequence_GetItem(pyObject, i));
            list.Add(item.As<TItem>());
        }

        return list;
    }

    private PyObject ConvertFromList(ICollection e)
    {
        List<PyObject> pyObjects = new(e.Count);

        foreach (object? item in e)
        {
            pyObjects.Add(ConvertFrom(item));
        }

        return Pack.CreateList(CollectionsMarshal.AsSpan(pyObjects));
    }

    private PyObject ConvertFromList(IEnumerable e)
    {
        List<PyObject> pyObjects = [];

        foreach (object? item in e)
        {
            pyObjects.Add(ConvertFrom(item));
        }

        return Pack.CreateList(CollectionsMarshal.AsSpan(pyObjects));
    }
}