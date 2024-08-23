using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Collections.Concurrent;
using System.Reflection;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter : TypeConverter
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

        if (destinationType == typeof(string) && CPythonAPI.IsPyUnicode(pyObject))
        {
            return CPythonAPI.PyUnicode_AsUTF8(pyObject);
        }

        if (destinationType == typeof(byte[]) && CPythonAPI.IsBytes(pyObject))
        {
            return CPythonAPI.PyBytes_AsByteArray(pyObject);
        }

        if (destinationType == typeof(long) && CPythonAPI.IsPyLong(pyObject))
        {
            return CPythonAPI.PyLong_AsLongLong(pyObject);
        }

        if (destinationType == typeof(BigInteger) && CPythonAPI.IsPyLong(pyObject))
        {
            return ConvertToBigInteger(pyObject, destinationType, context, culture);
        }

        if (destinationType == typeof(int) && CPythonAPI.IsPyLong(pyObject))
        {
            return CPythonAPI.PyLong_AsLong(pyObject);
        }

        if (destinationType == typeof(bool) && CPythonAPI.IsPyBool(pyObject))
        {
            return CPythonAPI.IsPyTrue(pyObject);
        }

        if (destinationType == typeof(double) && CPythonAPI.IsPyFloat(pyObject))
        {
            return CPythonAPI.PyFloat_AsDouble(pyObject);
        }

        if (destinationType.IsAssignableTo(typeof(ITuple)))
        {
            if (CPythonAPI.IsPyTuple(pyObject))
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

        if (destinationType.IsGenericType)
        {
            if (IsAssignableToGenericType(destinationType, dictionaryType) && CPythonAPI.IsPyDict(pyObject))
            {
                return ConvertToDictionary(pyObject, destinationType, context, culture);
            }

            if (IsAssignableToGenericType(destinationType, listType) && CPythonAPI.IsPyList(pyObject))
            {
                return ConvertToList(pyObject, destinationType, context, culture);
            }

            if (IsAssignableToGenericType(destinationType, generatorIteratorType) && CPythonAPI.IsPyGenerator(pyObject))
            {
                return ConvertToGeneratorIterator(pyObject, destinationType, context, culture);
            }

            // This needs to come after lists, because sequences are also maps
            if (destinationType.IsAssignableTo(collectionType) && CPythonAPI.IsPyMappingWithItems(pyObject))
            {
                return ConvertToDictionary(pyObject, destinationType, context, culture, useMappingProtocol: true);
            }

            if (IsAssignableToGenericType(destinationType, listType) && CPythonAPI.IsPySequence(pyObject))
            {
                return ConvertToListFromSequence(pyObject, destinationType, context, culture);
            }
        }

        throw new InvalidCastException($"Attempting to cast {destinationType} from {pyObject.GetPythonType()}");
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value switch
        {
            string str => PyObject.Create(CPythonAPI.AsPyUnicodeObject(str)),
            byte[] bytes => PyObject.Create(CPythonAPI.PyBytes_FromByteSpan(bytes.AsSpan())),
            long l => PyObject.Create(CPythonAPI.PyLong_FromLongLong(l)),
            int i => PyObject.Create(CPythonAPI.PyLong_FromLong(i)),
            bool b => PyObject.Create(CPythonAPI.PyBool_FromLong(b ? 1 : 0)),
            double d => PyObject.Create(CPythonAPI.PyFloat_FromDouble(d)),
            IDictionary dictionary => ConvertFromDictionary(context, culture, dictionary),
            ITuple t => ConvertFromTuple(context, culture, t),
            IEnumerable e => ConvertFromList(context, culture, e),
            BigInteger b => ConvertFromBigInteger(context, culture, b),
            PyObject pyObject => pyObject.Clone(),
            null => PyObject.None,
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
                IsAssignableToGenericType(destinationType, generatorIteratorType) ||
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
            return PyObject.None;
        }

        var result = ConvertFrom(context, culture, o);

        return result is null ? throw new NotImplementedException() : (PyObject)result;
    }

    record DynamicTypeInfo(ConstructorInfo ReturnTypeConstructor, ConstructorInfo? TransientTypeConstructor = null);
}
