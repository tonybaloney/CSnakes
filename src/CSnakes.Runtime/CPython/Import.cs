using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    /// <summary>
    /// Import a module and return a reference to it.
    /// </summary>
    /// <param name="name">The module name</param>
    /// <returns>A new reference to module `name`</returns>
    internal static PyObject Import(string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        nint module = PyImport_Import(pyName);
        Py_DecRefRaw(pyName);
        return PyObject.Create(module);
    }

    /// <summary>
    /// Import and return a reference to the module `name`
    /// </summary>
    /// <param name="name">The name of the module as a PyUnicode object.</param>
    /// <returns>A new reference to module `name`</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyImport_Import(nint name);
}
