using CSnakes.Runtime.Python;
using System.Collections;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    private object? ConvertToBigInteger(PyObject pyObject, Type destinationType, ITypeDescriptorContext? context, CultureInfo? culture, bool useMappingProtocol = false)
    {
        
    }

    private PyObject ConvertFromBigInteger(ITypeDescriptorContext? context, CultureInfo? culture, BigInteger integer)
    {
        byte[] integerBytes = integer.ToByteArray();
        using PyObject pyBytes = ConvertFromBytes(context, culture, integerBytes) as PyObject;
        return pyDict;
    }
}