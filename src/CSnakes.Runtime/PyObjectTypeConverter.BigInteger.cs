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
        using PythonObject pyUnicode = PythonObject.Create(CAPI.AsPyUnicodeObject(integer.ToString()));
        return PythonObject.Create(CAPI.PyLong_FromUnicodeObject(pyUnicode.DangerousGetHandle(), 10));
    }
}
