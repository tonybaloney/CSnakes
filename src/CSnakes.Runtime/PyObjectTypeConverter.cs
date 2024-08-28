using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Collections.Concurrent;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static readonly ConcurrentDictionary<Type, DynamicTypeInfo> knownDynamicTypes = [];

    public static object PyObjectToManagedType(PyObject pyObject, Type destinationType)
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
            return ConvertToListFromSequence(pyObject, destinationType);
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    public static PyObject ManagedTypeToPyObject(object value) =>
        value switch
        {
            string str => PyObject.Create(CPythonAPI.AsPyUnicodeObject(str)),
            byte[] bytes => PyObject.Create(CPythonAPI.PyBytes_FromByteSpan(bytes.AsSpan())),
            long l => PyObject.From(l),
            int i => PyObject.From(i),
            bool b => PyObject.From(b),
            double d => PyObject.From(d),
            IDictionary dictionary => ConvertFromDictionary(dictionary),
            ITuple t => ConvertFromTuple(t),
            ICollection l => ConvertFromList(l),
            IEnumerable e => ConvertFromList(e),
            BigInteger b => ConvertFromBigInteger(b),
            PyObject pyObject => pyObject.Clone(),
            null => PyObject.None,
            _ => throw new InvalidCastException($"Cannot convert {value} to PyObject")
        };

    record DynamicTypeInfo(ConstructorInfo ReturnTypeConstructor, ConstructorInfo? TransientTypeConstructor = null);
}
