using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyBoolType = IntPtr.Zero;
    private static nint Py_True = IntPtr.Zero;
    private static nint Py_False = IntPtr.Zero;

    public static bool IsPyBool(PythonObject p)
    {
        return p.DangerousGetHandle() == Py_True || p.DangerousGetHandle() == Py_False;
    }

    public static bool IsPyTrue(PythonObject p)
    {
        return p.DangerousGetHandle() == Py_True;
    }
}
