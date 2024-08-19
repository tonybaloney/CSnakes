using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter : TypeConverter
{

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
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is not PyObject pyObject)
        {
            throw new NotSupportedException();
        }

        if (destinationType == pyObjectType)
        {
            return pyObject.Clone();
        }

        nint handle = pyObject.GetHandle();
        if (destinationType == typeof(string) && CPythonAPI.IsPyUnicode(handle))
        {
            return CPythonAPI.PyUnicode_AsUTF8(handle);
        }

        if (destinationType == typeof(byte[]) && CPythonAPI.IsBytes(handle))
        {
            return CPythonAPI.PyBytes_AsByteArray(handle);
        }

        if (destinationType == typeof(long) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLongLong(handle);
        }

        if (destinationType == typeof(BigInteger) && CPythonAPI.IsPyLong(handle))
        {
            return ConvertToBigInteger(pyObject, destinationType, context, culture);
        }

        if (destinationType == typeof(int) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLongLong(handle);
        }

        if (destinationType == typeof(bool) && CPythonAPI.IsPyBool(handle))
        {
            return CPythonAPI.IsPyTrue(handle);
        }

        if (destinationType == typeof(double) && CPythonAPI.IsPyFloat(handle))
        {
            return CPythonAPI.PyFloat_AsDouble(handle);
        }

        if (destinationType.IsGenericType)
        {
            if (IsAssignableToGenericType(destinationType, dictionaryType) && CPythonAPI.IsPyDict(handle))
            {
                return ConvertToDictionary(pyObject, destinationType, context, culture);
            }

            if (IsAssignableToGenericType(destinationType, listType) && CPythonAPI.IsPyList(handle))
            {
                return ConvertToList(pyObject, destinationType, context, culture);
            }

            // This needs to come after lists, because sequences are also maps
            if (destinationType.IsAssignableTo(collectionType) && CPythonAPI.IsPyMappingWithItems(handle))
            {
                return ConvertToDictionary(pyObject, destinationType, context, culture, useMappingProtocol: true);
            }

            if (IsAssignableToGenericType(destinationType, listType) && CPythonAPI.IsPySequence(handle))
            {
                return ConvertToListFromSequence(pyObject, destinationType, context, culture);
            }

            if (destinationType.IsAssignableTo(typeof(ITuple)))
            {
                if (CPythonAPI.IsPyTuple(handle))
                {
                    return ConvertToTuple(context, culture, pyObject, destinationType);
                }

                var tupleTypes = destinationType.GetGenericArguments();
                if (tupleTypes.Length > 1)
                {
                    throw new InvalidCastException($"The type is a tuple with more than one generic argument, but the underlying Python type is not a tuple. Destination Type: {destinationType}");
                }

                var convertedValue = ConvertTo(context, culture, pyObject, tupleTypes[0]);
                return Activator.CreateInstance(destinationType, convertedValue);
            }
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    private object? AsManagedObject(Type type, PyObject p, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        return ConvertTo(context, culture, p, type);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value switch
        {
            string str => new PyObject(CPythonAPI.AsPyUnicodeObject(str)),
            byte[] bytes => new PyObject(CPythonAPI.PyBytes_FromByteSpan(bytes.AsSpan())),
            long l => new PyObject(CPythonAPI.PyLong_FromLongLong(l)),
            int i => new PyObject(CPythonAPI.PyLong_FromLong(i)),
            bool b => new PyObject(CPythonAPI.PyBool_FromLong(b ? 1 : 0)),
            double d => new PyObject(CPythonAPI.PyFloat_FromDouble(d)),
            IDictionary dictionary => ConvertFromDictionary(context, culture, dictionary),
            ITuple t => ConvertFromTuple(context, culture, t),
            IEnumerable e => ConvertFromList(context, culture, e),
            BigInteger b => ConvertFromBigInteger(context, culture, b),
            null => new PyObject(CPythonAPI.GetNone()),
            _ => base.ConvertFrom(context, culture, value)
        };

    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) =>
        destinationType is not null && (
            destinationType == typeof(string) ||
            destinationType == typeof(long) ||
            destinationType == typeof(int) ||
            destinationType == typeof(bool) ||
            destinationType == typeof(double) ||
            destinationType == typeof(byte[]) ||
            destinationType == typeof(BigInteger) ||
            (destinationType.IsGenericType && (
                IsAssignableToGenericType(destinationType, dictionaryType) ||
                IsAssignableToGenericType(destinationType, listType) ||
                destinationType.IsAssignableTo(collectionType) ||
                destinationType.IsAssignableTo(typeof(ITuple))
            ))
        );

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType is not null && (
            sourceType == typeof(string) ||
            sourceType == typeof(long) ||
            sourceType == typeof(int) ||
            sourceType == typeof(bool) ||
            sourceType == typeof(double) ||
            sourceType == typeof(byte[]) ||
            sourceType == typeof(BigInteger) ||
            (sourceType.IsGenericType && (
                IsAssignableToGenericType(sourceType, dictionaryType) ||
                IsAssignableToGenericType(sourceType, listType) ||
                sourceType.IsAssignableTo(collectionType) ||
                sourceType.IsAssignableTo(typeof(ITuple))
            ))
        );

    private PyObject ToPython(object? o, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        if (o is null)
        {
            return new PyObject(CPythonAPI.GetNone());
        }

        var result = ConvertFrom(context, culture, o);

        return result is null ? throw new NotImplementedException() : (PyObject)result;
    }
}
