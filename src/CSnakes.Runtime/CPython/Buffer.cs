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

    /// <summary>
    /// Represents <see href="https://github.com/python/cpython/blob/6280bb547840b609feedb78887c6491af75548e8/Include/pybuffer.h#L20-L33">
    /// <c>Py_buffer</c> from CPython</see>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Py_buffer
    {
        public void* /* void*       */ buf;
        public void* /* PyObject*   */ obj;
        public nint  /* Py_ssize_t  */ len;
        public nint  /* Py_ssize_t  */ itemsize;
        public int   /* int         */ @readonly;
        public int   /* int         */ ndim;
        public byte* /* char*       */ format;
        public nint* /* Py_ssize_t* */ shape;
        public nint* /* Py_ssize_t* */ strides;
        public nint* /* Py_ssize_t* */ suboffsets;
        public void* /* void*       */ @internal;
    }

    public static bool IsBuffer(PyObject p) => PyObject_CheckBuffer(p) == 1;

    internal static IPyBuffer GetBuffer(PyObject p) =>
        PyObject_GetBuffer(p, out var buffer, (int)(PyBUF.Format | PyBUF.CContiguous)) == 0
            ? buffer
            : throw PyObject.ThrowPythonExceptionAsClrException();

    internal static void ReleaseBuffer(ref Py_buffer view) => PyBuffer_Release(ref view);
    internal static void ReleaseBuffer(PyBuffer buffer) => PyBuffer_Release(buffer);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_CheckBuffer(PyObject ob);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_GetBuffer(PyObject ob, out PyBuffer view, int flags);

    [LibraryImport(PythonLibraryName)]
    private static partial void PyBuffer_Release(ref Py_buffer view);

    [LibraryImport(PythonLibraryName)]
    private static partial void PyBuffer_Release(PyBuffer view);
}
