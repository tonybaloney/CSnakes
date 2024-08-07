using CSnakes.Runtime.Convertors;
using CSnakes.Runtime.CPython;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Python;

[TypeConverter(typeof(PyObjectTypeConverter))]
public class PyObject : SafeHandle
{
    internal PyObject(IntPtr pyObject, bool ownsHandle = true) : base(pyObject, ownsHandle)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        CPythonAPI.Py_DecRef(handle);
        return true;
    }

    public PyObject Type()
    {
        // TODO: Handle releasing reference to the type object
        return new PyObject(CPythonAPI.GetType(DangerousGetHandle()));
    }

    /// <summary>
    /// Get the attribute of the object with name. This is equivalent to obj.name in Python.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Attribute object (new ref)</returns>
    public PyObject GetAttr(string name)
    {
        return new PyObject(CPythonAPI.GetAttr(handle, name));
    }

    /// <summary>
    /// Get the iterator for the object. This is equivalent to iter(obj) in Python.
    /// </summary>
    /// <returns>The iterator object (new ref)</returns>
    public PyObject GetIter()
    {
        return new PyObject(CPythonAPI.PyObject_GetIter(DangerousGetHandle()));
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

    public override string ToString()
    {
        var pyStringValue = CPythonAPI.PyObject_Str(handle);
        var stringValue = CPythonAPI.PyUnicode_AsUTF8(pyStringValue);
        CPythonAPI.Py_DecRef(pyStringValue);
        // TODO: Clear exception on null
        return stringValue ?? string.Empty;
    }

    public T As<T>()
    {
        throw new NotImplementedException();
    }

    internal object AsManagedObject(Type type)
    {
        throw new NotImplementedException();
    }
}

public static class Conversions
{
    public static PyObject ToPython(this object? o)
    {
        throw new NotImplementedException();
    }
}