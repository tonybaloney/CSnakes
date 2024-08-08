using CSnakes.Runtime.CPython;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Python;

[TypeConverter(typeof(PyObjectTypeConverter))]
public class PyObject : SafeHandle
{
    private static readonly TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));
    private bool hasDecrefed = false;

    internal PyObject(IntPtr pyObject, bool ownsHandle = true) : base(pyObject, ownsHandle)
    {
        if (pyObject == IntPtr.Zero)
        {
            hasDecrefed = true;
            ThrowPythonExceptionAsClrException();
        }
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!hasDecrefed)
        {
            CPythonAPI.Py_DecRef(handle);
            handle = IntPtr.Zero;
            hasDecrefed = true;
        } else
        {
            throw new AccessViolationException("Double free of PyObject");
        }
        return true;
    }

    private static void ThrowPythonExceptionAsClrException()
    {
        if (CPythonAPI.PyErr_Occurred() == 0)
        {
            throw new InvalidDataException("An error occurred in Python, but no exception was set.");
        }
        nint excType, excValue, excTraceback;
        CPythonAPI.PyErr_Fetch(out excType, out excValue, out excTraceback);
        var pyExceptionType = new PyObject(excType);
        var pyExceptionValue = new PyObject(excValue);
        var pyExceptionTraceback = new PyObject(excTraceback);

        if (pyExceptionType.IsInvalid || pyExceptionValue.IsInvalid || pyExceptionType.IsInvalid)
        {
            CPythonAPI.PyErr_Clear();
            throw new InvalidDataException("An error fetching the exceptions in Python.");
        }

        var pyExceptionStr = pyExceptionValue.ToString();
        var pyExceptionTypeStr = pyExceptionType.ToString();
        var pyExceptionTracebackStr = pyExceptionTraceback.ToString();
        pyExceptionType.Dispose();
        pyExceptionValue.Dispose();
        pyExceptionTraceback.Dispose();
        CPythonAPI.PyErr_Clear();
        throw new PythonException(pyExceptionTypeStr, pyExceptionStr, pyExceptionTracebackStr);
    }

    public PyObject Type()
    {
        Debug.Assert(!IsInvalid);
        return new PyObject(CPythonAPI.GetType(DangerousGetHandle()));
    }

    /// <summary>
    /// Get the attribute of the object with name. This is equivalent to obj.name in Python.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Attribute object (new ref)</returns>
    public PyObject GetAttr(string name)
    {
        Debug.Assert(!IsInvalid);
        return new PyObject(CPythonAPI.GetAttr(handle, name));
    }

    /// <summary>
    /// Get the iterator for the object. This is equivalent to iter(obj) in Python.
    /// </summary>
    /// <returns>The iterator object (new ref)</returns>
    public PyObject GetIter()
    {
        Debug.Assert(!IsInvalid);
        return new PyObject(CPythonAPI.PyObject_GetIter(DangerousGetHandle()));
    }

    public PyObject Call(params PyObject[] args)
    {
        Debug.Assert(!IsInvalid);
        var argHandles = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argHandles[i] = args[i].DangerousGetHandle();
        }
        return new PyObject(CPythonAPI.Call(DangerousGetHandle(), argHandles));
    }

    public override string ToString()
    {
        Debug.Assert(!IsInvalid);
        var pyStringValue = CPythonAPI.PyObject_Str(handle);
        var stringValue = CPythonAPI.PyUnicode_AsUTF8(pyStringValue);
        CPythonAPI.Py_DecRef(pyStringValue);
        // TODO: Clear exception on null
        return stringValue ?? string.Empty;
    }

    public T As<T>()
    {
        // TODO: This fails in many cases. 
        return (T) td.ConvertTo(this, typeof(T)) ?? default;
    }
}