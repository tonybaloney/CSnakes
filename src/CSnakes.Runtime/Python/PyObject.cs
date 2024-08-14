using CSnakes.Runtime.CPython;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Python;

[TypeConverter(typeof(PyObjectTypeConverter))]
public class PyObject : SafeHandle
{
    private static readonly TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));

    internal PyObject(IntPtr pyObject, bool ownsHandle = true) : base(pyObject, ownsHandle)
    {
        if (pyObject == IntPtr.Zero)
        {
            ThrowPythonExceptionAsClrException();
        }
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (IsInvalid)
            return true;
        if (!CPythonAPI.IsInitialized)
        {
            // The Python environment has been disposed, and therefore Python has freed it's memory pools.
            // Don't run decref since the Python process isn't running and this pointer will point somewhere else.
            handle = IntPtr.Zero;
            // TODO: Consider moving this to a logger.
            Debug.WriteLine($"Python object at 0x{handle:X} was released, but Python is no longer running.");
            return true;
        }
        using (GIL.Acquire())
        {
            CPythonAPI.Py_DecRef(handle);
        }
        handle = IntPtr.Zero;
        return true;
    }

    private static void ThrowPythonExceptionAsClrException()
    {
        using (GIL.Acquire())
        {
            if (CPythonAPI.PyErr_Occurred() == 0)
            {
                throw new InvalidDataException("An error occurred in Python, but no exception was set.");
            }
            CPythonAPI.PyErr_Fetch(out nint excType, out nint excValue, out nint excTraceback);
            using var pyExceptionType = new PyObject(excType);
            using var pyExceptionValue = new PyObject(excValue);
            using var pyExceptionTraceback = new PyObject(excTraceback);

            if (pyExceptionType.IsInvalid || pyExceptionValue.IsInvalid || pyExceptionType.IsInvalid)
            {
                CPythonAPI.PyErr_Clear();
                throw new InvalidDataException("An error fetching the exceptions in Python.");
            }

            var pyExceptionStr = pyExceptionValue.ToString();
            var pyExceptionTypeStr = pyExceptionType.ToString();
            var pyExceptionTracebackStr = pyExceptionTraceback.ToString();
            CPythonAPI.PyErr_Clear();
            throw new PythonException(pyExceptionTypeStr, pyExceptionStr, pyExceptionTracebackStr);
        }
    }

    /// <summary>
    /// Throw an invalid operation exception if Python is not running. This is used to prevent
    /// runtime errors when trying to use Python objects after the Python environment has been disposed.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static void RaiseOnPythonNotInitialized()
    {
        if (!CPythonAPI.IsInitialized)
        {
            throw new InvalidOperationException("Python is not initialized. You cannot call this method outside of a Python Environment context.");
        }
    }

    /// <summary>
    /// Get the type for the object.
    /// </summary>
    /// <returns>A new reference to the type field.</returns>
    public PyObject Type()
    {
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.GetType(DangerousGetHandle()));
        }
    }

    /// <summary>
    /// Get the attribute of the object with name. This is equivalent to obj.name in Python.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Attribute object (new ref)</returns>
    public PyObject GetAttr(string name)
    {
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.GetAttr(handle, name));
        }
    }

    /// <summary>
    /// Get the iterator for the object. This is equivalent to iter(obj) in Python.
    /// </summary>
    /// <returns>The iterator object (new ref)</returns>
    public PyObject GetIter()
    {
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.PyObject_GetIter(DangerousGetHandle()));
        }
    }

    /// <summary>
    /// Call the object. Equivalent to (__call__)(args)
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The resulting object, or NULL on error.</returns>
    public PyObject Call(params PyObject[] args)
    {
        RaiseOnPythonNotInitialized();
        // TODO: Consider moving this to a logger.
        Debug.Assert(!IsInvalid);
        // TODO: Stack allocate short parameter lists (<10?)
        var argHandles = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argHandles[i] = args[i].DangerousGetHandle();
        }
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.Call(DangerousGetHandle(), argHandles));
        }
    }

    /// <summary>
    /// Get a string representation of the object.
    /// </summary>
    /// <returns>The result of `str()` on the object.</returns>
    public override string ToString()
    {
        // TODO: Consider moving this to a logger.
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PyObject pyObjectStr = new(CPythonAPI.PyObject_Str(handle));
            string? stringValue = CPythonAPI.PyUnicode_AsUTF8(pyObjectStr.DangerousGetHandle());
            return stringValue ?? string.Empty;
        }
    }

    public T As<T>()
    {
        return (T)td.ConvertTo(this, typeof(T)) ?? default;
    }

    internal PyObject Clone()
    {
        CPythonAPI.Py_IncRef(handle);
        return new PyObject(handle);
    }
}