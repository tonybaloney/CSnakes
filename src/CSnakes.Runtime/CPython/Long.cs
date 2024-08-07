using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static nint PyLongType = IntPtr.Zero;

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyLong_FromLong(long v);

    [LibraryImport(PythonLibraryName)]
    internal static partial long PyLong_AsLong(nint p);

    internal static bool IsPyLong(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyLongType;
    }
}
