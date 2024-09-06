using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    [Flags]
    private enum PyBUF
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
        public IntPtr buf;
        public IntPtr obj;
        public nint len;
        public nint itemsize;
        public int @readonly;
        public Int32 ndim;
        public byte* format;
        public nint* shape;
        public nint* strides;
        public nint* suboffsets;
        public nint* @internal;
    }

    public static bool IsBuffer(PyObject p) => PyObject_CheckBuffer(p) == 1;

    internal static Py_buffer GetBuffer(PyObject p)
    {
        Py_buffer view = default;
        if (PyObject_GetBuffer(p, &view, (int)(PyBUF.Format | PyBUF.CContiguous)) != 0)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        return view;
    }

    internal static void ReleaseBuffer(Py_buffer view)
    {
        PyBuffer_Release(&view);
    }

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_CheckBuffer(PyObject ob);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_GetBuffer(PyObject ob, Py_buffer* view, int flags);

    [LibraryImport(PythonLibraryName)]
    private static partial void PyBuffer_Release(Py_buffer* view);
}
