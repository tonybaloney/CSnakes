using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static PyObject* Import(string name)
    {
        PyObject* pyName = AsPyUnicodeObject(name);
        PyObject* module = PyImport_Import(pyName);
        Py_DecRef(pyName);
        return module;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial PyObject* PyImport_Import(PyObject* name);
}
