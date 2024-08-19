namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyNone = IntPtr.Zero;

    /// <summary>
    /// Get the None object.
    /// </summary>
    /// <returns>A new reference to None. In newer versions of Python, None is immortal anyway.</returns>
    internal static nint GetNone()
    {
        Py_IncRefRaw(PyNone);
        return PyNone;
    }
}
