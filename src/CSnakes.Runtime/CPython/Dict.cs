using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static nint PyDictType;

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyDict_New();

    public static bool IsPyDict(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyDictType;
    }

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
