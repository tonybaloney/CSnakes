using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    private const int PyBUF_SIMPLE = 0;
    private const int PyBUF_WRITABLE = 0x0001;
    private const int PyBUF_FORMAT = 0x0004;
    private const int PyBUF_ND = 0x0008;
    private const int PyBUF_STRIDES = (0x0010 | PyBUF_ND);
    private const int PyBUF_C_CONTIGUOUS = (0x0020 | PyBUF_STRIDES);
    private const int PyBUF_F_CONTIGUOUS = (0x0040 | PyBUF_STRIDES);
    private const int PyBUF_ANY_CONTIGUOUS = (0x0080 | PyBUF_STRIDES);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Py_buffer
    {
        public IntPtr buf;
        public IntPtr obj;
        public nint len;
        public int @readonly;
        public nint itemsize;
        public byte* format;
        public Int32 ndim;
        public nint* shape;
        public nint* strides;
        public nint* suboffsets;
        public nint* @internal;
    }

    public static bool IsBuffer(PyObject p)
    {
        return PyObject_CheckBuffer(p) == 1;
    }

    internal static Py_buffer GetBuffer(PyObject p)
    {
        Py_buffer view = default;
        if (PyObject_GetBuffer(p, &view, PyBUF_FORMAT) != 0)
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
