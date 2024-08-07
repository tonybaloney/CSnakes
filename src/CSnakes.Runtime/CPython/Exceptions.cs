using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{

    // Borrowed reference
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyErr_Occurred();

    [LibraryImport(PythonLibraryName)]
    internal static partial void PyErr_Clear();

    // https://docs.python.org/3.10/c-api/exceptions.html#c.PyErr_Fetch
    [LibraryImport(PythonLibraryName)]
    internal static partial void PyErr_Fetch(out nint ptype, out nint pvalue, out nint ptraceback);
}
