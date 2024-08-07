using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_New();

    public static bool IsPyDict(nint p)
    {
        return PyDict_CheckExact(p) == 1 || PyDict_Check(p) == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyDict_Check(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyDict_CheckExact(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Size(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_GetItem(nint p, nint key);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyDict_SetItem(nint p, nint key, nint value);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Items(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Keys(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_Values(nint p);
}
