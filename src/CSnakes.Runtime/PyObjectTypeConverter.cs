using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections.Concurrent;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static readonly ConcurrentDictionary<Type, DynamicTypeInfo> knownDynamicTypes = [];

    public static object PyObjectToManagedType(PythonObject pyObject, Type destinationType)
    {
        if (CAPI.IsPyDict(pyObject) && IsAssignableToGenericType(destinationType, dictionaryType))
        {
            return ConvertToDictionary(pyObject, destinationType);
        }

        if (CAPI.IsPyList(pyObject) && IsAssignableToGenericType(destinationType, listType))
        {
            return ConvertToList(pyObject, destinationType);
        }

        // This needs to come after lists, because sequences are also maps
        if (CAPI.IsPyMappingWithItems(pyObject) && destinationType.IsAssignableTo(collectionType))
        {
            return ConvertToDictionary(pyObject, destinationType, useMappingProtocol: true);
        }

        if (CAPI.IsPySequence(pyObject) && IsAssignableToGenericType(destinationType, listType))
        {
            return ConvertToList(pyObject, destinationType);
        }

        if (CAPI.IsBuffer(pyObject) && destinationType.IsAssignableTo(bufferType))
        {
            return new PyBuffer(pyObject);
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    record DynamicTypeInfo(ConstructorInfo ReturnTypeConstructor, ConstructorInfo? TransientTypeConstructor = null);
}
