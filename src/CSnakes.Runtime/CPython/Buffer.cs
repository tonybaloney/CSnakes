using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    public static bool IsBuffer(MPyOPtr p) => PyObject_CheckBuffer(p.DangerousGetHandle()) == 1;

    internal static PyBuffer GetBuffer(MPyOPtr p)
    {
        PyBuffer? view = GetBuffer(p.DangerousGetHandle());
        if (view == null)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        else
        {
            return (Unmanaged.CAPI.PyBuffer)view;
        }
    }

}
