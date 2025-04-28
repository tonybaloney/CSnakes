using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static readonly ConcurrentDictionary<Type, DynamicTypeInfo> knownDynamicTypes = [];

    public static object PyObjectToManagedType(PyObject pyObject, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type destinationType)
    {
        if (CPythonAPI.IsPyDict(pyObject) && IsAssignableToGenericType(destinationType, dictionaryType))
        {
            return ConvertToDictionary(pyObject, destinationType);
        }

        if (CPythonAPI.IsPyList(pyObject) && IsAssignableToGenericType(destinationType, listType))
        {
            return ConvertToList(pyObject, destinationType);
        }

        // This needs to come after lists, because sequences are also maps
        if (CPythonAPI.IsPyMappingWithItems(pyObject) && destinationType.IsAssignableTo(collectionType))
        {
            return ConvertToDictionary(pyObject, destinationType, useMappingProtocol: true);
        }

        if (CPythonAPI.IsPySequence(pyObject) && IsAssignableToGenericType(destinationType, listType))
        {
            return ConvertToList(pyObject, destinationType);
        }

        if (CPythonAPI.IsBuffer(pyObject) && destinationType.IsAssignableTo(bufferType))
        {
            return new PyBuffer(pyObject);
        }

        if (CPythonAPI.IsHeapType(pyObject.GetPythonType()))
        {
            return ConvertToClass(pyObject, destinationType);
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    record DynamicTypeInfo(ConstructorInfo ReturnTypeConstructor, ConstructorInfo? TransientTypeConstructor = null);
}
