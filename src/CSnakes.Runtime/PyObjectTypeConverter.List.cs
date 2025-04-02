using CSnakes.Runtime.Python;
using System.Collections;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object ConvertToList(PyObject pyObject, Type destinationType)
    {
        Type genericArgument = destinationType.GetGenericArguments()[0];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type listType = typeof(PyList<>).MakeGenericType(genericArgument);
            typeInfo = new(listType.GetConstructor([typeof(PyObject)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }

    internal static PyObject ConvertFromList(ICollection e)
    {
        List<PyObject> pyObjects = new(e.Count);

        foreach (object? item in e)
        {
            pyObjects.Add(PyObject.From(item));
        }

        return Pack.CreateList(CollectionsMarshal.AsSpan(pyObjects));
    }

    internal static PyObject ConvertFromList(IEnumerable e)
    {
        List<PyObject> pyObjects = [];

        foreach (object? item in e)
        {
            pyObjects.Add(PyObject.From(item));
        }

        return Pack.CreateList(CollectionsMarshal.AsSpan(pyObjects));
    }
}