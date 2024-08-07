using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyLong_FromLong(nint v);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyLong_AsLong(nint p);

    internal static bool IsPyLong(nint p)
    {
        return PyLong_CheckExact(p) == 1 || PyLong_Check(p) == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyLong_Check(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyLong_CheckExact(nint p);
}
