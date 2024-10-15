using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyLongType = IntPtr.Zero;

    /// <summary>
    /// Calls PyLong_AsLongLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long PyLong_AsLongLong(PythonObject p)
    {
        long result = PyLong_AsLongLong(p.DangerousGetHandle());
        if (result == -1 && IsPyErrOccurred())
        {
            throw PythonObject.ThrowPythonExceptionAsClrException("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
        }
        return result;
    }

    /// <summary>
    /// Calls PyLong_AsLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long PyLong_AsLong(PythonObject p)
    {
        long result = PyLong_AsLong(p.DangerousGetHandle());
        if (result == -1 && IsPyErrOccurred())
        {
            throw PythonObject.ThrowPythonExceptionAsClrException("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
        }
        return result;
    }

    internal static bool IsPyLong(PythonObject p)
    {
        return PyObject_IsInstance(p, PyLongType);
    }

}
