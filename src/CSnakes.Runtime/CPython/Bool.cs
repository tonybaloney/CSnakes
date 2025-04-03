using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyBoolType = IntPtr.Zero;
    private static nint Py_True = IntPtr.Zero;
    private static nint Py_False = IntPtr.Zero;

    /// <summary>
    /// Convert a Int32 to a PyBool Object
    /// </summary>
    /// <param name="value">Numeric value (0 or 1)</param>
    /// <returns>New reference to a PyBool Object</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyBool_FromLong(nint value);

    public static bool IsPyBool(PyObject p)
    {
        return p.DangerousGetHandle() == Py_True || p.DangerousGetHandle() == Py_False;
    }

    public static bool IsPyTrue(PyObject p)
    {
        return p.DangerousGetHandle() == Py_True;
    }
}
