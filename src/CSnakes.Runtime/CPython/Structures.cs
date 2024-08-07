namespace CSnakes.Runtime.CPython;

using Py_ssize_t = System.IntPtr;

internal unsafe partial class CPythonAPI
{
    internal struct PyObjectStruct
    {
        Py_ssize_t ob_refcnt;
        PyObjectStruct* ob_type;

        internal readonly IntPtr Type() => (nint)ob_type;
    }

    internal struct PyVarObject
    {
        PyObjectStruct* ob_base;
        Py_ssize_t ob_size;
    }
}
