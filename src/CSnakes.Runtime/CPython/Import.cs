using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static IntPtr Import(string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        nint module = PyImport_Import(pyName);
        Py_DecRef(pyName);
        return module;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyImport_Import(nint name);
}
