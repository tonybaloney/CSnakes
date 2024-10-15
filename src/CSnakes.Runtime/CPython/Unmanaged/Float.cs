using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    protected static pyoPtr _PyFloatType = IntPtr.Zero;
    public static pyoPtr PtrToPyFloatType => _PyFloatType;

    internal static double DoubleFromPyFloat(pyoPtr p)
    {
        double result = PyFloat_AsDouble(p);
        if (result == -1 && PyErr_Occurred() != IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException("Error converting Python object to double, check that the object was a Python float. See InnerException for details.");
        }
        return result;
    }

}
