using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{

    /// <summary>
    /// Has an error occured. Caller must hold the GIL.
    /// </summary>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyErr_Occurred();

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
