using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    private static nint PyBytesType = IntPtr.Zero;

    public static bool IsBytes(PythonObject p)
    {
        return PyObject_IsInstance(p, PyBytesType);
    }

    internal static nint PyBytes_FromByteSpan(Span<byte> bytes)
    {
        fixed (byte* b = bytes)
        {
            return PyBytes_FromStringAndSize(b, bytes.Length);
        }
    }

    internal static byte[] PyBytes_AsByteArray(PythonObject bytes)
    {
        byte* ptr = PyBytes_AsString(bytes);
        nint size = PyBytes_Size(bytes);
        byte[] byteArray = new byte[size];
        Marshal.Copy((IntPtr)ptr, byteArray, 0, (int)size);
        return byteArray;
    }


    [LibraryImport(PythonLibraryName)]
    private static partial byte* PyBytes_AsString(PythonObject ob);

    [LibraryImport(PythonLibraryName)]
    private static partial nint PyBytes_Size(PythonObject ob);
}
