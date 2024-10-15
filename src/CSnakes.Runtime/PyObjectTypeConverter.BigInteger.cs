using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Numerics;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    internal static BigInteger ConvertToBigInteger(PythonObject pyObject, Type destinationType) =>
        // There is no practical API for this in CPython. Use str() instead. 
        BigInteger.Parse(pyObject.ToString());

    internal static PythonObject ConvertFromBigInteger(BigInteger integer)
    {
        using PythonObject pyUnicode = PythonObject.Create(CPythonAPI.AsPyUnicodeObject(integer.ToString()));
        return PythonObject.Create(CPythonAPI.PyLong_FromUnicodeObject(pyUnicode.DangerousGetHandle(), 10));
    }
}
