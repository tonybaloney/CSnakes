using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyFloatType = IntPtr.Zero;

    /// <summary>
    /// Create a PyFloat from the C double
    /// </summary>
    /// <param name="v"></param>
    /// <returns>A new reference to the float object</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyFloat_FromDouble(double value);

    internal static double PyFloat_AsDouble(PyObject p)
    {
        double result = PyFloat_AsDouble_(p);
        if (result == -1 && PyErr_Occurred())
        {
            throw PyObject.ThrowPythonExceptionAsClrException("Error converting Python object to double, check that the object was a Python float. See InnerException for details.");
        }
        return result;
    }

    /// <summary>
    /// Convery a PyFloat to a C double
    /// </summary>
    /// <param name="p"></param>
    /// <returns>The double value</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyFloat_AsDouble")]
    private static partial double PyFloat_AsDouble_(PyObject obj);

    internal static bool IsPyFloat(PyObject p)
    {
        return PyObject_IsInstance(p, PyFloatType);
    }
}
