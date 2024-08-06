using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyGILState_Ensure();

    [LibraryImport(PythonLibraryName)]
    internal static partial void PyGILState_Release(nint state);
}
