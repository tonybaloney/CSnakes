using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    protected static pyoPtr GetBuiltin(string name)
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
