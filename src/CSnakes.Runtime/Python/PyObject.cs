using CSnakes.Runtime.CPython;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

    /// <summary>
    /// Gets the handle, or raise exception if it has been marked as invalid.
    /// ALWAYS use this instead of DangerousGetHandle().
    /// </summary>
    /// <returns></returns>
    internal IntPtr GetHandle()
    {
        if (handle == IntPtr.Zero)
            throw new ObjectDisposedException($"Unable to get reference to PyObject. Object has already been disposed or is invalid.");
        return handle;
    }

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
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.GetType(GetHandle()));
        }
    }

    /// <summary>
    /// Get the attribute of the object with name. This is equivalent to obj.name in Python.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Attribute object (new ref)</returns>
    public PyObject GetAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.GetAttr(GetHandle(), name));
        }
    }

    public bool HasAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return CPythonAPI.HasAttr(GetHandle(), name);
        }
    }

    /// <summary>
    /// Get the iterator for the object. This is equivalent to iter(obj) in Python.
    /// </summary>
    /// <returns>The iterator object (new ref)</returns>
    public PyObject GetIter()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.PyObject_GetIter(GetHandle()));
        }
    }

    /// <summary>
    /// Get the results of the repr() function on the object.
    /// </summary>
    /// <returns></returns>
    public string GetRepr()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PyObject reprStr = new PyObject(CPythonAPI.PyObject_Repr(GetHandle()));
            string? repr = CPythonAPI.PyUnicode_AsUTF8(reprStr.GetHandle());
            return repr ?? string.Empty;
        }
    }

    /// <summary>
    /// Call the object. Equivalent to (__call__)(args)
    /// All arguments are treated as positional.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The resulting object, or NULL on error.</returns>
    public PyObject Call(params PyObject[] args)
    {
        RaiseOnPythonNotInitialized();
        // TODO: Consider moving this to a logger.
        // TODO: Stack allocate short parameter lists (<10?)
        var argHandles = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argHandles[i] = args[i].GetHandle();
        }
        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.Call(GetHandle(), argHandles));
        }
    }

    public PyObject CallWithArgs(PyObject[]? args = null)
    {
        RaiseOnPythonNotInitialized();
        args ??= [];
        int argsLen = args?.Length ?? 0;
        var argHandles = new IntPtr[argsLen];

        for (int i = 0; i < argsLen; i++)
        {
            argHandles[i] = args![i].GetHandle();
        }

        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.Call(GetHandle(), argHandles));
        }
    }

    public PyObject CallWithKeywordArguments(PyObject[]? args = null, string[]? kwnames = null, PyObject[]? kwvalues = null)
    {
        if (kwnames == null)
            return CallWithArgs(args);
        RaiseOnPythonNotInitialized();
        int argsLen = args?.Length ?? 0;
        var argHandles = new IntPtr[argsLen];

        for (int i = 0; i < argsLen; i++)
        {
            argHandles[i] = args![i].GetHandle();
        }

        var kwargHandles = new IntPtr[kwnames.Length];
        for (int i = 0; i < kwnames.Length; i++)
        {
            kwargHandles[i] = kwvalues![i].GetHandle();
        }

        using (GIL.Acquire())
        {
            return new PyObject(CPythonAPI.Call(GetHandle(), argHandles, kwnames, kwargHandles));
        }
    }

    public PyObject CallWithKeywordArguments(PyObject[]? args = null, string[]? kwnames = null, PyObject[]? kwvalues = null, IReadOnlyDictionary<string, PyObject>? kwargs = null)
    {
        // No keyword parameters supplied
        if (kwnames == null && kwargs == null)
            return CallWithArgs(args);
        // Keyword args are empty and kwargs is empty. 
        if (kwnames != null && kwnames.Length == 0 && (kwargs == null || kwargs.Count == 0))
            return CallWithArgs(args);

        MergeKeywordArguments(kwnames ?? [], kwvalues ?? [], kwargs, out string[] combinedKwnames, out PyObject[] combinedKwvalues);
        return CallWithKeywordArguments(args, combinedKwnames, combinedKwvalues);
    }

    /// <summary>
    /// Get a string representation of the object.
    /// </summary>
    /// <returns>The result of `str()` on the object.</returns>
    public override string ToString()
    {
        // TODO: Consider moving this to a logger.
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PyObject pyObjectStr = new(CPythonAPI.PyObject_Str(GetHandle()));
            string? stringValue = CPythonAPI.PyUnicode_AsUTF8(pyObjectStr.GetHandle());
            return stringValue ?? string.Empty;
        }
    }

    public T As<T>() => (T)(td.ConvertTo(this, typeof(T)) ?? default!);

    internal PyObject Clone()
    {
        CPythonAPI.Py_IncRef(GetHandle());
        return new PyObject(GetHandle());
    }

    private static void MergeKeywordArguments(string[] kwnames, PyObject[] kwvalues, IReadOnlyDictionary<string, PyObject>? kwargs, out string[] combinedKwnames, out PyObject[] combinedKwvalues)
    {
        if (kwargs == null)
        {
            combinedKwnames = kwnames;
            combinedKwvalues = kwvalues;
            return;
        }

        var newKwnames = new List<string>(kwnames);
        var newKwvalues = new List<PyObject>(kwvalues);

        // The order must be the same as we're not submitting these in a mapping, but a parallel array.
        foreach (var (key, value) in kwargs)
        {
            newKwnames.Add(key);
            newKwvalues.Add(value);
        }

        combinedKwnames = [.. newKwnames];
        combinedKwvalues = [.. newKwvalues];
    }
}