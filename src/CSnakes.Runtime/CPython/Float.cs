using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyFloat_FromDouble(double v);


    [LibraryImport(PythonLibraryName)]
    internal static partial double PyFloat_AsDouble(nint p);

    internal static bool IsPyFloat(nint p)
    {
        return PyFloat_CheckExact(p) == 1 || PyFloat_Check(p) == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyFloat_Check(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyFloat_CheckExact(nint p);
}
