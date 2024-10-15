using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    private static nint PyBytesType = IntPtr.Zero;

    public static bool IsBytes(PyObject p)
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

    internal static byte[] PyBytes_AsByteArray(PyObject bytes)
    {
        byte* ptr = PyBytes_AsString(bytes);
        nint size = PyBytes_Size(bytes);
        byte[] byteArray = new byte[size];
        Marshal.Copy((IntPtr)ptr, byteArray, 0, (int)size);
        return byteArray;
    }


    [LibraryImport(PythonLibraryName)]
    private static partial byte* PyBytes_AsString(PyObject ob);

    [LibraryImport(PythonLibraryName)]
    private static partial nint PyBytes_Size(PyObject ob);
}
