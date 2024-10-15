using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Import a module and return a reference to it.
    /// </summary>
    /// <param name="name">The module name</param>
    /// <returns>A new reference to module `name`</returns>
    internal static PythonObject Import(string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        nint module = PyImport_Import(pyName);
        Py_DecRef(pyName);
        return PythonObject.Create(module);
    }

    protected static nint GetBuiltin(string name)
    {
        nint pyName = AsPyUnicodeObject("builtins");
        nint pyAttrName = AsPyUnicodeObject(name);
        nint module = PyImport_Import(pyName);
        nint attr = PyObject_GetAttr(module, pyAttrName);
        if (attr == IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        Py_DecRef(pyName);
        Py_DecRef(pyAttrName);
        return attr;
    } 
}
