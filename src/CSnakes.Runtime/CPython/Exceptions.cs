using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{

    /// <summary>
    /// Has an error occured. Caller must hold the GIL.
    /// </summary>
    /// <returns></returns>
    internal static bool PyErr_Occurred()
    {
        return PyErr_Occurred_() != IntPtr.Zero;
    }

    /// <summary>
    /// Keep this function private. Use PyErr_Occurred() instead.
    /// It returns a borrowed reference to the exception object. USe PyErr_Fetch if you need the exception.
    /// </summary>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyErr_Occurred")]
    private static partial nint PyErr_Occurred_();

    [LibraryImport(PythonLibraryName)]
    internal static partial void PyErr_Clear();

    // https://docs.python.org/3.10/c-api/exceptions.html#c.PyErr_Fetch
    /// <summary>
    /// Retrieve the error indicator into three variables whose addresses are passed. 
    /// If the error indicator is not set, set all three variables to NULL. 
    /// If it is set, it will be cleared and you own a reference to each object retrieved. 
    /// The value and traceback object may be NULL even when the type object is not.
    /// </summary>
    /// <param name="ptype"></param>
    /// <param name="pvalue"></param>
    /// <param name="ptraceback"></param>
    [LibraryImport(PythonLibraryName)]
    internal static partial void PyErr_Fetch(out nint ptype, out nint pvalue, out nint ptraceback);
}
