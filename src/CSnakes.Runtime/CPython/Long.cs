using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyLongType = IntPtr.Zero;

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyLong_FromLong(int v);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyLong_FromLongLong(long v);

    [LibraryImport(PythonLibraryName)]
    internal static partial long PyLong_AsLongLong(PyObject p);

    internal static bool IsPyLong(PyObject p)
    {
        return PyObject_IsInstance(p, PyLongType);
    }
}
