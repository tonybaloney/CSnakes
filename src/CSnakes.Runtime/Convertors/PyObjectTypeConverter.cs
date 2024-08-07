using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
            return CPythonAPI.PyLong_AsLong(handle);
        }

        if (destinationType == typeof(int) && CPythonAPI.IsPyLong(handle))
        {
            return CPythonAPI.PyLong_AsLong(handle);
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
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    private object? ConvertToList(PyObject pyObject, Type destinationType)
    {
        throw new NotImplementedException();
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value switch
        {
            string str => new PyObject(CPythonAPI.AsPyUnicodeObject(str)),
            long l => new PyObject(CPythonAPI.PyLong_FromLong(l)),
            int i => new PyObject(CPythonAPI.PyLong_FromLong(i)),
            bool b => new PyObject(CPythonAPI.PyBool_FromLong(b ? 1 : 0)),
            double d => new PyObject(CPythonAPI.PyFloat_FromDouble(d)),
            _ => base.ConvertFrom(context, culture, value)
        };

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
}
