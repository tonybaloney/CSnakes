using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyListType = IntPtr.Zero;

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyList_New(nint size);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyList_Size(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyList_GetItem(nint p, nint pos);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyList_SetItem(nint p, nint pos, nint o);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyList_Append(nint p, nint o);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyList_Insert(nint p, nint pos, nint o);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyList_Extend(nint p, nint o);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyList_Reverse(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyList_Sort(nint p);

    internal static bool IsPyList(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyListType;
    }
}
