using CSnakes.Runtime.CPython;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

[DebuggerDisplay("PyObject: repr={GetRepr()}, type={GetPythonType().ToString()}")]
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
            CPythonAPI.Py_DecRefRaw(handle);
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
            
            if (excType == 0)
            {
                throw new InvalidDataException("An error occurred in Python, but no exception was set.");
            }

            using var pyExceptionType = new PyObject(excType);
            PyObject? pyExceptionTraceback = excTraceback == IntPtr.Zero ? null : new PyObject(excTraceback);

            var pyExceptionStr = string.Empty;
            if (excValue != IntPtr.Zero)
            {
                using PyObject pyExceptionValue = new PyObject(excValue);
                pyExceptionStr = pyExceptionValue.ToString();
            }
             ;
            // TODO: Consider adding __qualname__ as well for module exceptions that aren't builtins
            var pyExceptionTypeStr = pyExceptionType.GetAttr("__name__").ToString();
            CPythonAPI.PyErr_Clear();
            throw new PythonInvocationException(pyExceptionTypeStr, pyExceptionStr, pyExceptionTraceback);
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
    public PyObject GetPythonType()
    {
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.GetType(this));
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
            return new PyObject(CPythonAPI.GetAttr(this, name));
        }
    }

    public bool HasAttr(string name)
    {
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return CPythonAPI.HasAttr(this, name);
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
            return new PyObject(CPythonAPI.PyObject_GetIter(this));
        }
    }

    /// <summary>
    /// Get the results of the repr() function on the object.
    /// </summary>
    /// <returns></returns>
    public string GetRepr()
    {
        Debug.Assert(!IsInvalid);
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PyObject reprStr = new PyObject(CPythonAPI.PyObject_Repr(this));
            string? repr = CPythonAPI.PyUnicode_AsUTF8(reprStr);
            return repr ?? string.Empty;
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
        var marshallers = new SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn[args.Length];
        var argHandles = args.Length < 16
            ? stackalloc IntPtr[args.Length]
            : new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            ref var m = ref marshallers[i];
            m.FromManaged(args[i]);
            argHandles[i] = m.ToUnmanaged();
        }
        try
        {
            using (GIL.Acquire())
            {
                return new PyObject(CPythonAPI.Call(this, argHandles));
            }
        }
        finally
        {
            foreach (var m in marshallers)
            {
                m.Free();
            }
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
            using PyObject pyObjectStr = new(CPythonAPI.PyObject_Str(this));
            string? stringValue = CPythonAPI.PyUnicode_AsUTF8(pyObjectStr);
            return stringValue ?? string.Empty;
        }
    }

    public T As<T>() => (T)(td.ConvertTo(this, typeof(T)) ?? default!);

    internal PyObject Clone()
    {
        CPythonAPI.Py_IncRefRaw(handle);
        return new PyObject(handle);
    }
}