using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{

    /// <summary>
    /// Has an error occured. Caller must hold the GIL.
    /// </summary>
    /// <returns></returns>
    internal static bool IsPyErrOccurred()
    {
        return PyErr_Occurred() != IntPtr.Zero;
    }

}
