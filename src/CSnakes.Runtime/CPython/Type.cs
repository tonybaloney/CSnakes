using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private const uint Py_TPFLAGS_HEAPTYPE = 1u << 9;

    internal static bool IsHeapType(PyObject ob)
    {
        return (PyType_GetFlags(ob) & Py_TPFLAGS_HEAPTYPE) != 0;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial uint PyType_GetFlags(PyObject ob);
}
