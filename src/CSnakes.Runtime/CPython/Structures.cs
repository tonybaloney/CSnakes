using System.Runtime.InteropServices;


namespace CSnakes.Runtime;

using Py_ssize_t = System.IntPtr;

internal unsafe partial class CPythonAPI
{
    struct PyObject
    {
        Py_ssize_t ob_refcnt;
        PyObject* ob_type;
    }

    struct PyVarObject
    {
        PyObject ob_base;
        Py_ssize_t ob_size;
    }
}
