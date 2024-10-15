namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    protected static pyoPtr _PyBoolType = IntPtr.Zero;
    protected static pyoPtr _PyTrue = IntPtr.Zero;
    protected static pyoPtr _PyFalse = IntPtr.Zero;

    public static pyoPtr PtrToPyBoolType => _PyBoolType;
    public static pyoPtr PtrToPyTrue => _PyTrue;
    public static pyoPtr PtrToPyFalse => _PyFalse;
}
