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
        byte[] integerBytes = integer.ToByteArray(isBigEndian: true, isUnsigned: false);
        using PyObject pyBytes = new PyObject(CPythonAPI.PyBytes_FromByteSpan(integerBytes.AsSpan()));
        PyObject intObject = new PyObject(CPythonAPI.PyLong_FromBytes(pyBytes.GetHandle()));
        return intObject;
    }
}