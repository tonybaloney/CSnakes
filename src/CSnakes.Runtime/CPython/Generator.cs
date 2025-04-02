using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static bool IsPyGenerator(PyObject p)
    {
        // TODO : Find a reference to a generator object.
        return HasAttr(p, "__next__") && HasAttr(p, "send");
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyGen_New(PyObject frame);
}
