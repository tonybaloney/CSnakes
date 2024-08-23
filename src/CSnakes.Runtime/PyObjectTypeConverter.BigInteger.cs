using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Numerics;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private static object? ConvertToBigInteger(PyObject pyObject, Type destinationType) =>
        // There is no practical API for this in CPython. Use str() instead. 
        BigInteger.Parse(pyObject.ToString());

    private static PyObject ConvertFromBigInteger(BigInteger integer)
    {
        using PyObject pyUnicode = PyObject.Create(CPythonAPI.AsPyUnicodeObject(integer.ToString()));
        return PyObject.Create(CPythonAPI.PyLong_FromUnicodeObject(pyUnicode, 10));
    }
}