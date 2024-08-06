namespace CSnakes.Runtime.CPython;

using Py_ssize_t = System.IntPtr;

internal unsafe partial class CPythonAPI
{
    internal struct PyObject
    {
        Py_ssize_t ob_refcnt;
        PyObject* ob_type;
    }

    internal struct PyVarObject
    {
        PyObject ob_base;
        Py_ssize_t ob_size;
    }
}
