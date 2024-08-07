using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    private static nint PyTupleType = IntPtr.Zero;
    private static nint PyEmptyTuple = IntPtr.Zero;

    internal static nint PackTuple(params nint[] items)
    {
        if (items.Length == 0)
        {
            Py_IncRef(PyEmptyTuple);
            return PyEmptyTuple;
        }

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
    internal static partial nint PyTuple_SetItem(nint ob, nint pos, nint o);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_GetItem(nint ob, nint pos);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_Size(nint p);

    internal static bool IsPyTuple(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyTupleType;
    }
}
