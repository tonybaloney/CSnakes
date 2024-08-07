using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime.Convertors;
internal class PyObjectTypeConverter : TypeConverter
{
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is not PyObject pyObject)
        {
            throw new NotSupportedException();
        }

        nint handle = pyObject.DangerousGetHandle();
        if (destinationType == typeof(string) && CPythonAPI.IsPyUnicode(handle))
        {
            return CPythonAPI.PyUnicode_AsUTF8(handle);
        }

        if (destinationType == typeof(long) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLongLong(handle);
        }

        if (destinationType == typeof(int) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLongLong(handle);
        }

        if (destinationType == typeof(bool) && CPythonAPI.IsPyBool(handle))
        {
            return CPythonAPI.PyBool_FromLong(handle);
        }

        if (destinationType == typeof(double) && CPythonAPI.IsPyFloat(handle))
        {
            return CPythonAPI.PyFloat_AsDouble(handle);
        }

        if (destinationType.IsGenericType)
        {
            if (destinationType.GetGenericTypeDefinition() == typeof(IEnumerable<>) && CPythonAPI.IsPyList(handle))
            {
                return ConvertToList(pyObject, destinationType);
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
                    throw new NotSupportedException("The type is a tuple with more than one generic argument, but the underlying Python type is not a tuple.");
                }

                var convertedValue = ConvertTo(context, culture, pyObject, tupleTypes[0]);
                return Activator.CreateInstance(destinationType, convertedValue);
            }
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    private object? ConvertToTuple(ITypeDescriptorContext? context, CultureInfo? culture, PyObject pyObj, Type destinationType)
    {
        var tuplePtr = pyObj.DangerousGetHandle();

        // We have to convert the Python values to CLR values, as if we just tried As<object>() it would
        // not parse the Python type to a CLR type, only to a new Python type.
        Type[] types = destinationType.GetGenericArguments();
        object?[] clrValues;

        var tupleValues = new List<PyObject>();
        for (nint i = 0; i < CPythonAPI.PyTuple_Size(tuplePtr); i++)
        {
            // TODO: Inspect GetItem returns borrowed or new reference.
            tupleValues.Add(new PyObject(CPythonAPI.PyTuple_GetItem(tuplePtr, i)));
        }

        // If we have more than 8 python values, we are going to have a nested tuple, which we have to unpack.
        if (tupleValues.Count > 8)
        {
            // We are hitting nested tuples here, which will be treated in a different way.
            object?[] firstSeven = tupleValues.Take(7).Select((p, i) => AsManagedObject(types[i], p, context, culture)).ToArray();

            // Get the rest of the values and convert them to a nested tuple.
            IEnumerable<PyObject> rest = tupleValues.Skip(7);

            // Back to a Python tuple.
            PyObject pyTuple = PyTuple.CreateTuple(rest);

            // Use the decoder pipeline to decode the nested tuple (and its values).
            // We do this because that means if we have nested nested tuples, they'll be decoded as well.
            object? nestedTuple = AsManagedObject(types[7], pyTuple, context, culture);

            // Append our nested tuple to the first seven values.
            clrValues = [.. firstSeven, nestedTuple];
        }
        else
        {
            clrValues = tupleValues.Select((p, i) => AsManagedObject(types[i], p, context, culture)).ToArray();
        }

        ConstructorInfo ctor = destinationType.GetConstructors().First(c => c.GetParameters().Length == clrValues.Length);
        return (ITuple)ctor.Invoke(clrValues);
    }

    private object? AsManagedObject(Type type, PyObject p, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        return ConvertTo(context, culture, p, type);
    }

    private object? ConvertToList(PyObject pyObject, Type destinationType)
    {
        throw new NotImplementedException();
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value switch
        {
            string str => new PyObject(CPythonAPI.AsPyUnicodeObject(str)),
            long l => new PyObject(CPythonAPI.PyLong_FromLongLong(l)),
            int i => new PyObject(CPythonAPI.PyLong_FromLongLong(i)),
            bool b => new PyObject(CPythonAPI.PyBool_FromLong(b ? 1 : 0)),
            double d => new PyObject(CPythonAPI.PyFloat_FromDouble(d)),
            ITuple t => ConvertFromTuple(context, culture, t),
            _ => base.ConvertFrom(context, culture, value)
        };

    private PyObject ConvertFromTuple(ITypeDescriptorContext? context, CultureInfo? culture, ITuple t)
    {
        List<PyObject> pyObjects = [];

        for (var i = 0; i < t.Length; i++)
        {
            var currentValue = t[i];

            if (currentValue is null)
            {
                // TODO: handle null values
            }

            pyObjects.Add(ToPython(currentValue, context, culture));
        }

        return PyTuple.CreateTuple(pyObjects);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) =>
        destinationType is not null && (
            destinationType == typeof(string) ||
            destinationType == typeof(long) ||
            destinationType == typeof(int) ||
            destinationType == typeof(bool) ||
            destinationType == typeof(double) ||
            destinationType.IsGenericType
        );

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType is not null && (
            sourceType == typeof(string) ||
            sourceType == typeof(long) ||
            sourceType == typeof(int) ||
            sourceType == typeof(bool) ||
            sourceType == typeof(double) ||
            sourceType.IsGenericType
        );

    private PyObject ToPython(object o, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        var result = ConvertFrom(context, culture, o);

        if (result is null)
        {
            throw new NotImplementedException();
        }

        return (PyObject)result;
    }
}
