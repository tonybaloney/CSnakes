using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    /// <summary>
    /// Ensure (hold) GIL and return a GIL state handle (ptr)
    /// Handles are not memory managed by Python. Do not free them.
    /// </summary>
    /// <returns>A GIL state handle</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyGILState_Ensure();

    /// <summary>
    /// Release the GIL with the given state handle
    /// </summary>
    /// <param name="state">GIL State handle</param>
    [LibraryImport(PythonLibraryName)]
    internal static partial void PyGILState_Release(nint state);
}
