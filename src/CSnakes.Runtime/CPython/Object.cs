using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Repr(IntPtr ob);

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_DecRef(nint ob);

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_IncRef(nint ob);

    internal static IntPtr GetType(IntPtr ob) => ((PyObjectStruct*)ob)->Type();

    internal static IntPtr GetAttr(IntPtr ob, string name)
    {
        /* TODO: Consider interning/caching the name value */
        nint pyName = AsPyUnicodeObject(name);
        nint pyAttr = PyObject_GetAttr(ob, pyName);
        Py_DecRef(pyName);
        return pyAttr;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetAttr(IntPtr ob, IntPtr attr);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_HasAttr(IntPtr ob, IntPtr attr);

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetIter(IntPtr ob);

    /*    PyAPI_FUNC(PyObject*) PyObject_Str(PyObject*);
        PyAPI_FUNC(PyObject*) PyObject_ASCII(PyObject*);
        PyAPI_FUNC(PyObject*) PyObject_Bytes(PyObject*);
        PyAPI_FUNC(PyObject*) PyObject_RichCompare(PyObject*, PyObject*, int);
        PyAPI_FUNC(int) PyObject_RichCompareBool(PyObject*, PyObject*, int);
        PyAPI_FUNC(PyObject*) PyObject_GetAttrString(PyObject*, const char*);
        PyAPI_FUNC(int) PyObject_SetAttrString(PyObject*, const char*, PyObject *);
        PyAPI_FUNC(int) PyObject_HasAttrString(PyObject*, const char*);
        PyAPI_FUNC(PyObject*) PyObject_GetAttr(PyObject*, PyObject*);
        PyAPI_FUNC(int) PyObject_SetAttr(PyObject*, PyObject*, PyObject*);
        PyAPI_FUNC(int) PyObject_HasAttr(PyObject*, PyObject*);*/
}
