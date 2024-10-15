using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    public static bool IsBuffer(PythonObject p) => PyObject_CheckBuffer(p.DangerousGetHandle()) == 1;

    internal static PyBuffer GetBuffer(PythonObject p)
    {
        PyBuffer view = default;
        if (PyObject_GetBuffer(p.DangerousGetHandle(), &view, (int)(PyBUF.Format | PyBUF.CContiguous)) != 0)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return view;
    }

    internal static void ReleaseBuffer(PyBuffer view) => PyBuffer_Release(&view);
}
