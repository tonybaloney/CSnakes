using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python.Interns;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

[DebuggerDisplay("PyObject: repr={GetRepr()}, type={GetPythonType().ToString()}")]
public class PyObject : SafeHandle
{
    private static readonly PyObjectTypeConverter td = new();
    private PyObject[]? disposeCollection;

    protected PyObject(IntPtr pyObject, bool ownsHandle = true) : base(pyObject, ownsHandle)
    {
        if (pyObject == IntPtr.Zero)
        {
            ThrowPythonExceptionAsClrException();
        }
    }

    internal static PyObject Create(IntPtr ptr)
    {
        if (None.DangerousGetHandle() == ptr)
            return None;
        return new PyObject(ptr);
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

    protected override void Dispose(bool disposing)
    {
        if (disposeCollection is not null)
        {
            foreach (var obj in disposeCollection)
            {
                obj.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Throws a Python exception as a CLR exception.
    /// </summary>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="PythonInvocationException"></exception>
    internal static void ThrowPythonExceptionAsClrException(string? message = null)
    {
        using (GIL.Acquire())
        {
            if (!CPythonAPI.PyErr_Occurred())
            {
                throw new InvalidDataException("An error occurred in Python, but no exception was set.");
            }
            CPythonAPI.PyErr_Fetch(out nint excType, out nint excValue, out nint excTraceback);

            if (excType == 0)
            {
                throw new InvalidDataException("An error occurred in Python, but no exception was set.");
            }

            using var pyExceptionType = Create(excType);
            PyObject? pyExceptionTraceback = excTraceback == IntPtr.Zero ? null : new PyObject(excTraceback);

            var pyExceptionStr = string.Empty;
            if (excValue != IntPtr.Zero)
            {
                using PyObject pyExceptionValue = Create(excValue);
                pyExceptionStr = pyExceptionValue.ToString();
            }
             ;
            // TODO: Consider adding __qualname__ as well for module exceptions that aren't builtins
            var pyExceptionTypeStr = pyExceptionType.GetAttr("__name__").ToString();
            CPythonAPI.PyErr_Clear();

            if (string.IsNullOrEmpty(message))
            {
                throw new PythonInvocationException(pyExceptionTypeStr, pyExceptionStr, pyExceptionTraceback);
            }
            else
            {
                throw new PythonInvocationException(pyExceptionTypeStr, pyExceptionStr, pyExceptionTraceback, message);
            }
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
    public virtual PyObject GetPythonType()
    {
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
    public virtual PyObject GetAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(CPythonAPI.GetAttr(this, name));
        }
    }

    public virtual bool HasAttr(string name)
    {
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
    public virtual PyObject GetIter()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(CPythonAPI.PyObject_GetIter(this));
        }
    }

    /// <summary>
    /// Get the results of the repr() function on the object.
    /// </summary>
    /// <returns></returns>
    public virtual string GetRepr()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PyObject reprStr = new PyObject(CPythonAPI.PyObject_Repr(this));
            string? repr = CPythonAPI.PyUnicode_AsUTF8(reprStr);
            return repr ?? string.Empty;
        }
    }

    /// <summary>
    /// Is the Python object None?
    /// </summary>
    /// <returns>true if None, else false</returns>
    public virtual bool IsNone() => CPythonAPI.IsNone(this);

    /// <summary>
    /// Are the objects the same instance, equivalent to the `is` operator in Python
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Is(PyObject other)
    {
        return DangerousGetHandle() == other.DangerousGetHandle();
    }

    public override bool Equals(object? obj)
    {
        if (obj is PyObject pyObj1)
        {
            if (Is(pyObj1))
                return true;
            return Compare(this, pyObj1, CPythonAPI.RichComparisonType.Equal);
        }
        return base.Equals(obj);
    }

    public bool NotEquals(object? obj)
    {
        if (obj is PyObject pyObj1)
        {
            if (Is(pyObj1))
                return false;
            return Compare(this, pyObj1, CPythonAPI.RichComparisonType.NotEqual);
        }
        return !base.Equals(obj);
    }

    public static bool operator ==(PyObject? left, PyObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Equals(right),
        };
    }

    public static bool operator !=(PyObject? left, PyObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => true,
            (null, _) => true,
            (_, _) => left.NotEquals(right),
        };
    }

    public static bool operator <=(PyObject? left, PyObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, CPythonAPI.RichComparisonType.LessThanEqual),
        };
    }

    public static bool operator >=(PyObject? left, PyObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, CPythonAPI.RichComparisonType.GreaterThanEqual),
        };
    }

    public static bool operator <(PyObject? left, PyObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, CPythonAPI.RichComparisonType.LessThan),
        };
    }

    public static bool operator >(PyObject? left, PyObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, CPythonAPI.RichComparisonType.GreaterThan),
        };
    }

    private static bool Compare(PyObject left, PyObject right, CPythonAPI.RichComparisonType type)
    {
        using (GIL.Acquire())
        {
            return CPythonAPI.PyObject_RichCompare(left, right, type);
        }
    }

    public override int GetHashCode()
    {
        using (GIL.Acquire())
        {
            int hash = CPythonAPI.PyObject_Hash(this);
            if (hash == -1)
            {
                ThrowPythonExceptionAsClrException();
            }
            return hash;
        }
    }

    public static PyObject None { get; } = new PyNoneObject();
    public static PyObject True { get; } = new PyTrueObject();
    public static PyObject False { get; } = new PyFalseObject();
    public static PyObject MinusOne { get; } = new PySmallIntObject(-1);
    public static PyObject Zero { get; } = new PySmallIntObject(0);
    public static PyObject One { get; } = new PySmallIntObject(1);


    /// <summary>
    /// Call the object. Equivalent to (__call__)(args)
    /// All arguments are treated as positional.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The resulting object, or NULL on error.</returns>
    public PyObject Call(params PyObject[] args)
    {
        return CallWithArgs(args);
    }

    public PyObject CallWithArgs(PyObject[]? args = null)
    {
        RaiseOnPythonNotInitialized();

        // Don't do any marshalling if there aren't any arguments. 
        if (args is null || args.Length == 0)
        {
            using (GIL.Acquire())
            {
                return Create(CPythonAPI.PyObject_CallNoArgs(this));
            }
        }

        args ??= [];
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
                return Create(CPythonAPI.Call(this, argHandles));
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

    public PyObject CallWithKeywordArguments(PyObject[]? args = null, string[]? kwnames = null, PyObject[]? kwvalues = null)
    {
        if (kwnames is null)
            return CallWithArgs(args);
        if (kwvalues is null || kwnames.Length != kwvalues.Length)
            throw new ArgumentException("kwnames and kwvalues must be the same length.");
        RaiseOnPythonNotInitialized();
        args ??= [];

        var argMarshallers = new SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn[args.Length];
        var kwargMarshallers = new SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn[kwvalues.Length];
        var argHandles = args.Length < 16
            ? stackalloc IntPtr[args.Length]
            : new IntPtr[args.Length];
        var kwargHandles = kwvalues.Length < 16
            ? stackalloc IntPtr[kwvalues.Length]
            : new IntPtr[kwvalues.Length];

        for (int i = 0; i < args.Length; i++)
        {
            ref var m = ref argMarshallers[i];
            m.FromManaged(args[i]);
            argHandles[i] = m.ToUnmanaged();
        }
        for (int i = 0; i < kwvalues.Length; i++)
        {
            ref var m = ref kwargMarshallers[i];
            m.FromManaged(kwvalues[i]);
            kwargHandles[i] = m.ToUnmanaged();
        }

        try
        {
            using (GIL.Acquire())
            {
                return Create(CPythonAPI.Call(this, argHandles, kwnames, kwargHandles));
            }
        }
        finally
        {
            foreach (var m in argMarshallers)
            {
                m.Free();
            }
            foreach (var m in kwargMarshallers)
            {
                m.Free();
            }
        }
    }

    public PyObject CallWithKeywordArguments(PyObject[]? args = null, string[]? kwnames = null, PyObject[]? kwvalues = null, IReadOnlyDictionary<string, PyObject>? kwargs = null)
    {
        // No keyword parameters supplied
        if (kwnames is null && kwargs is null)
            return CallWithArgs(args);
        // Keyword args are empty and kwargs is empty. 
        if (kwnames is not null && kwnames.Length == 0 && (kwargs is null || kwargs.Count == 0))
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
            using PyObject pyObjectStr = new(CPythonAPI.PyObject_Str(this));
            string? stringValue = CPythonAPI.PyUnicode_AsUTF8(pyObjectStr);
            return stringValue ?? string.Empty;
        }
    }

    public T As<T>()
    {
        using (GIL.Acquire())
        {
            return (T)(td.ConvertTo(this, typeof(T)) ?? default!);
        }
    }

    public static PyObject? From<T>(T value)
    {
        using (GIL.Acquire())
        {
            return value is null ?
                PyObject.None :
                (PyObject?)td.ConvertFrom(value);
        }
    }

    // Overload value types to avoid boxing in the generic method.
    public static PyObject From(long value)
    {
        using (GIL.Acquire())
        {
            if (value == -1) return MinusOne;
            if (value == 0) return Zero;
            if (value == 1) return One;
            return Create(CPythonAPI.PyLong_FromLongLong(value))!;
        }
    }

    public static PyObject From(int value)
    {
        using (GIL.Acquire())
        {
            if (value == -1) return MinusOne;
            if (value == 0) return Zero;
            if (value == 1) return One;
            return Create(CPythonAPI.PyLong_FromLong(value))!;
        }
    }

    public static PyObject From(double value)
    {
        using (GIL.Acquire())
        {
            return Create(CPythonAPI.PyFloat_FromDouble(value))!;
        }
    }

    public static PyObject From(bool value)
    {
        using (GIL.Acquire())
        {
            return value ? True : False;
        }
    }

    internal virtual PyObject Clone()
    {
        CPythonAPI.Py_IncRefRaw(handle);
        return new PyObject(handle);
    }

    private static void MergeKeywordArguments(string[] kwnames, PyObject[] kwvalues, IReadOnlyDictionary<string, PyObject>? kwargs, out string[] combinedKwnames, out PyObject[] combinedKwvalues)
    {
        if (kwnames.Length != kwvalues.Length)
            throw new ArgumentException("kwnames and kwvalues must be the same length.");
        if (kwargs is null)
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

    internal PyObject RegisterDisposeCollection(PyObject[] collection)
    {
        disposeCollection = collection;
        return this;
    }
}