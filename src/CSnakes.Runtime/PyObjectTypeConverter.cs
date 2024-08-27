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
    private readonly ConcurrentDictionary<Type, DynamicTypeInfo> knownDynamicTypes = [];

    /// <summary>
    /// Convert a Python object to a CLR managed object.
    /// Caller should already hold the GIL because this function uses the Python runtime for some conversions.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="culture"></param>
    /// <param name="value"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Passed object is not a PyObject</exception>
    /// <exception cref="InvalidCastException">Source/Target types do not match</exception>
    public object ConvertTo(object? value, Type destinationType)
    {
        if (value is not PyObject pyObject)
        {
            throw new NotSupportedException();
        }

        if (destinationType == pyObjectType)
        {
            return pyObject.Clone();
        }

        if (destinationType == typeof(byte[]) && CPythonAPI.IsBytes(pyObject))
        {
            return CPythonAPI.PyBytes_AsByteArray(pyObject);
        }

        if (destinationType == typeof(long))
        {
            long result = CPythonAPI.PyLong_AsLongLong(pyObject);
            if (result == -1 && CPythonAPI.PyErr_Occurred())
            {
                throw PyObject.ThrowPythonExceptionAsClrException("Error converting Python object to long, check that the object was a Python long or that the value wasn't too large. See InnerException for details.");
            }
            return result;
        }

        if (destinationType == typeof(BigInteger) && CPythonAPI.IsPyLong(pyObject))
        {
            return ConvertToBigInteger(pyObject, destinationType);
        }

        if (destinationType == typeof(int))
        {
            var result = CPythonAPI.PyLong_AsLong(pyObject);
            if (result == -1 && CPythonAPI.PyErr_Occurred())
            {
                throw PyObject.ThrowPythonExceptionAsClrException("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
            }
            return result;
        }

        if (destinationType == typeof(bool) && CPythonAPI.IsPyBool(pyObject))
        {
            return CPythonAPI.IsPyTrue(pyObject);
        }

        if (destinationType == typeof(double))
        {
            var result = CPythonAPI.PyFloat_AsDouble(pyObject);
            if (result == -1 && CPythonAPI.PyErr_Occurred())
            {
                throw PyObject.ThrowPythonExceptionAsClrException("Error converting Python object to double, check that the object was a Python float. See InnerException for details.");
            }
            return result;
        }

        if (destinationType.IsAssignableTo(typeof(ITuple)))
        {
            if (CPythonAPI.IsPyTuple(pyObject))
            {
                return ConvertToTuple(pyObject, destinationType);
            }

            var tupleTypes = destinationType.GetGenericArguments();
            if (tupleTypes.Length > 1)
            {
                throw new InvalidCastException($"The type is a tuple with more than one generic argument, but the underlying Python type is not a tuple. Destination Type: {destinationType}");
            }

            var convertedValue = ConvertTo(pyObject, tupleTypes[0]);
            return Activator.CreateInstance(destinationType, convertedValue)!;
        }

        if (destinationType.IsGenericType)
        {
            if (IsAssignableToGenericType(destinationType, dictionaryType) && CPythonAPI.IsPyDict(pyObject))
            {
                return ConvertToDictionary(pyObject, destinationType);
            }

            if (IsAssignableToGenericType(destinationType, listType) && CPythonAPI.IsPyList(pyObject))
            {
                return ConvertToList(pyObject, destinationType);
            }

            if (IsAssignableToGenericType(destinationType, generatorIteratorType) && CPythonAPI.IsPyGenerator(pyObject))
            {
                return ConvertToGeneratorIterator(pyObject, destinationType);
            }

            // This needs to come after lists, because sequences are also maps
            if (destinationType.IsAssignableTo(collectionType) && CPythonAPI.IsPyMappingWithItems(pyObject))
            {
                return ConvertToDictionary(pyObject, destinationType, useMappingProtocol: true);
            }

            if (IsAssignableToGenericType(destinationType, listType) && CPythonAPI.IsPySequence(pyObject))
            {
                return ConvertToListFromSequence(pyObject, destinationType);
            }
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    public PyObject ConvertFrom(object value) =>
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

    private PyObject ToPython(object? o)
    {
        if (o is null)
        {
            return PyObject.None;
        }

        var result = ConvertFrom(o);

        return result is null ? throw new NotImplementedException() : result;
    }

    record DynamicTypeInfo(ConstructorInfo ReturnTypeConstructor, ConstructorInfo? TransientTypeConstructor = null);
}
