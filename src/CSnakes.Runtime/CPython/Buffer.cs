using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
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
