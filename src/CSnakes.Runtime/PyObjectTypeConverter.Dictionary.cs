using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Diagnostics;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object ConvertToDictionary(PyObject pyObject, Type destinationType, bool useMappingProtocol = false)
    {
        Type keyType = destinationType.GetGenericArguments()[0];
        Type valueType = destinationType.GetGenericArguments()[1];

        if (!knownDynamicTypes.TryGetValue(destinationType, out DynamicTypeInfo? typeInfo))
        {
            Type dictType = typeof(PyDictionary<,>).MakeGenericType(keyType, valueType);

            typeInfo = new(dictType.GetConstructor([typeof(PyObject)])!);
            knownDynamicTypes[destinationType] = typeInfo;
        }

        return typeInfo.ReturnTypeConstructor.Invoke([pyObject.Clone()]);
    }

    internal static PyObject ConvertFromDictionary(IDictionary dictionary)
    {
        PyObject pyDict = PyObject.Create(CPythonAPI.PyDict_New());

        foreach (DictionaryEntry kvp in dictionary)
        {
            int result = CPythonAPI.PyDict_SetItem(pyDict, PyObject.From(kvp.Key), PyObject.From(kvp.Value));
            if (result == -1)
            {
                throw PyObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyDict;
    }
}