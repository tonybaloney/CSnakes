using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    [Flags]
    private enum PyBUF : int
    {
        Simple = 0,
        Writable = 0x1,
        Format = 0x4,
        ND = 0x8,
        Strides = 0x10 | ND,
        CContiguous = 0x20 | Strides,
        FContiguous = 0x40 | Strides,
        AnyContiguous = 0x80 | Strides,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Py_buffer
    {
        public nint buf;
        public nint obj;
        public nint len;
        public nint itemsize;
        public int @readonly;
        public int ndim;
        public byte* format;
        public nint* shape;
        public nint* strides;
        public nint* suboffsets;
        public nint* @internal;
    }

    public static bool IsBuffer(PyObject p) => PyObject_CheckBuffer(p) == 1;

    internal static void GetBuffer(PyObject p, out Py_buffer buffer)
    {
        if (PyObject_GetBuffer(p, out buffer, (int)(PyBUF.Format | PyBUF.CContiguous)) != 0)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
    }

    internal static void ReleaseBuffer(ref Py_buffer view) => PyBuffer_Release(ref view);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_CheckBuffer(PyObject ob);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_GetBuffer(PyObject ob, out Py_buffer view, int flags);

    [LibraryImport(PythonLibraryName)]
    private static partial void PyBuffer_Release(ref Py_buffer view);
}
