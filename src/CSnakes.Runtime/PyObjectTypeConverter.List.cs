using CSnakes.Runtime.Python;
using System.Collections;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object ConvertToList(PythonObject pyObject, Type destinationType)
    {
        Type genericArgument = destinationType.GetGenericArguments()[0];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type listType = typeof(PyList<>).MakeGenericType(genericArgument);
            typeInfo = new(listType.GetConstructor([typeof(PythonObject)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }

    internal static PythonObject ConvertFromList(ICollection e)
    {
        List<PythonObject> pyObjects = new(e.Count);

        foreach (object? item in e)
        {
            pyObjects.Add(PythonObject.From(item));
        }

        return Pack.CreateList(CollectionsMarshal.AsSpan(pyObjects));
    }

    internal static PythonObject ConvertFromList(IEnumerable e)
    {
        List<PythonObject> pyObjects = [];

        foreach (object? item in e)
        {
            pyObjects.Add(PythonObject.From(item));
        }

        return Pack.CreateList(CollectionsMarshal.AsSpan(pyObjects));
    }
}