using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToBigInteger(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture)
    {
        // There is no practical API for this in CPython. Use str() instead. 
        return BigInteger.Parse(pyObject.ToString());
    }

    private PyObject ConvertFromBigInteger(ITypeDescriptorContext? context, CultureInfo? culture, BigInteger integer)
    {
        using PyObject pyUnicode = new PyObject(CPythonAPI.AsPyUnicodeObject(integer.ToString()));
        return new PyObject(CPythonAPI.PyLong_FromUnicodeObject(pyUnicode.GetHandle(), 10));
    }
}