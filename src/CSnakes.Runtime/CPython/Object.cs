using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial PyObject* PyObject_Repr(PyObject* ob);


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
