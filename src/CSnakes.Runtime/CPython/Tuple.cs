using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static nint PackTuple(params nint[] items)
    {
        nint tuple = PyTuple_New(items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            PyTuple_SetItem(tuple, i, items[i]);
        }
        return tuple;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_New(nint size);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_SetItem(nint p, nint pos, nint o);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_GetItem(nint p, nint pos);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_Size(nint p);
}
