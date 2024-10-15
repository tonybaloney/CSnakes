using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    internal static pyoPtr ByteSpanToPyBytes(Span<byte> bytes)
    {
        fixed (byte* b = bytes)
        {
            return PyBytes_FromStringAndSize(b, bytes.Length);
        }
    }

    internal static byte[] ByteArrayFromPyBytes(pyoPtr pyBytesPtr)
    {
        byte* ptr = PyBytes_AsString(pyBytesPtr);
        nint size = PyBytes_Size(pyBytesPtr);
        byte[] byteArray = new byte[size];
        Marshal.Copy((IntPtr)ptr, byteArray, 0, (int)size);
        return byteArray;
    }
}
