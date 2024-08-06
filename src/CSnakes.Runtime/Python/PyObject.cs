using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Python;

public class PyObject : SafeHandle
{
    internal PyObject(IntPtr pyObject) : base(pyObject, true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        CPythonAPI.Py_DecRef(handle);
        return true;
    }

    public PyObject GetAttr(string name)
    {
        return new PyObject(CPythonAPI.GetAttr(handle, name));
    }

    public PyObject Call(params PyObject[] args)
    {
        var argHandles = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argHandles[i] = args[i].DangerousGetHandle();
        }

        return new PyObject(CPythonAPI.Call(DangerousGetHandle(), argHandles));
    }
}

