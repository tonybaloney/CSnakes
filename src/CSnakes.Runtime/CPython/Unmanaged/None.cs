namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Get the None object.
    /// </summary>
    /// <returns>A new reference to None. In newer versions of Python, None is immortal anyway.</returns>
    internal static pyoPtr GetNone()
    {
        if (_PyNone == IntPtr.Zero)
        {
            throw new InvalidOperationException("Python is not initialized. You cannot call this method outside of a Python Environment context.");
        }
        Py_IncRef(_PyNone);
        return _PyNone;
    }
}
