using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyBool_FromLong(long value);

    public static bool IsPyBool(nint p)
    {
        return PyBool_Check(p) == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyBool_Check(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint Py_False();

    [LibraryImport(PythonLibraryName)]
    internal static partial nint Py_True();


}
