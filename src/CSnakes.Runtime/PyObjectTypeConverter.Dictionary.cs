using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object ConvertToDictionary(PythonObject pyObject, Type destinationType, bool useMappingProtocol = false)
    {
        Type keyType = destinationType.GetGenericArguments()[0];
        Type valueType = destinationType.GetGenericArguments()[1];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type dictType = typeof(PyDictionary<,>).MakeGenericType(keyType, valueType);

            typeInfo = new(dictType.GetConstructor([typeof(PythonObject)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }

    internal static PythonObject ConvertFromDictionary(IDictionary dictionary)
    {
        PythonObject pyDict = PythonObject.Create(CPythonAPI.PyDict_New());

        foreach (DictionaryEntry kvp in dictionary)
        {
            int result = CPythonAPI.PyDict_SetItem(pyDict, PythonObject.From(kvp.Key), PythonObject.From(kvp.Value));
            if (result == -1)
            {
                throw PythonObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyDict;
    }
}