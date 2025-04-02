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

    /// <summary>
    /// Calls PyLong_AsLongLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long PyLong_AsLongLong(PyObject p)
    {
        long result = PyLong_AsLongLong_(p);
        if (result == -1 && PyErr_Occurred())
        {
            throw PyObject.ThrowPythonExceptionAsClrException("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
        }
        return result;
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyLong_AsLongLong")]
    private static partial long PyLong_AsLongLong_(PyObject p);

    /// <summary>
    /// Calls PyLong_AsLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long PyLong_AsLong(PyObject p)
    {
        long result = PyLong_AsLong_(p);
        if (result == -1 && PyErr_Occurred())
        {
            throw PyObject.ThrowPythonExceptionAsClrException("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
        }
        return result;
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyLong_AsLong")]
    private static partial int PyLong_AsLong_(PyObject p);

    internal static bool IsPyLong(PyObject p)
    {
        return PyObject_IsInstance(p, PyLongType);
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyLong_FromUnicodeObject(PyObject unicode, int @base);
}
