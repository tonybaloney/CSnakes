using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python.Interns;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

[DebuggerDisplay("PyObject: repr={GetRepr()}, type={GetPythonType().ToString()}")]
public partial class PyObject : SafeHandle, ICloneable
{
    protected PyObject(IntPtr pyObject, bool ownsHandle = true) : base(pyObject, ownsHandle)
    {
        if (pyObject == IntPtr.Zero)
        {
            throw ThrowPythonExceptionAsClrException();
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
        if (GIL.IsAcquired)
        {
            using (GIL.Acquire())
            {
                CPythonAPI.Py_DecRefRaw(handle);
            }
        }
        else
        {
            // Probably in the GC finalizer thread, instead of causing GIL contention, put this on a queue to be processed later.
            GIL.QueueForDisposal(handle);
        }
        handle = IntPtr.Zero;
        return true;
    }

    /// <summary>
    /// Throws a Python exception as a CLR exception.
    /// </summary>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="PythonInvocationException"></exception>
    internal static Exception ThrowPythonExceptionAsClrException(string? message = null)
    {
        using (GIL.Acquire())
        {
            if (!CPythonAPI.PyErr_Occurred())
            {
                return new InvalidDataException("An error occurred in Python, but no exception was set.");
            }
            CPythonAPI.PyErr_Fetch(out nint excType, out nint excValue, out nint excTraceback);

            if (excType == 0)
            {
                return new InvalidDataException("An error occurred in Python, but no exception was set.");
            }

            using var pyExceptionType = Create(excType);
            PyObject? pyExceptionTraceback = excTraceback == IntPtr.Zero ? null : new PyObject(excTraceback);
            PyObject? pyException = excValue == IntPtr.Zero ? null : Create(excValue);

            // TODO: Consider adding __qualname__ as well for module exceptions that aren't builtins
            var pyExceptionTypeStr = pyExceptionType.GetAttr("__name__").ToString();
            CPythonAPI.PyErr_Clear();

            if (string.IsNullOrEmpty(message))
            {
                return new PythonInvocationException(pyExceptionTypeStr, pyException, pyExceptionTraceback);
            }

            return new PythonInvocationException(pyExceptionTypeStr, pyException, pyExceptionTraceback, message);
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
    /// <param name="name">Attribute name</param>
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


    internal virtual PyObject GetIter()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(CPythonAPI.PyObject_GetIter(this));
        }
    }

    /// <summary>
    /// Calls iter() on the object and returns an IEnumerable that yields values of type T.
    /// </summary>
    /// <typeparam name="T">The type for each item in the iterator</typeparam>
    /// <returns></returns>
    public IEnumerable<T> AsEnumerable<T>()
    {
        using (GIL.Acquire())
        {
            return new PyEnumerable<T>(this);
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
            return CPythonAPI.PyUnicode_AsUTF8(reprStr);
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
                throw ThrowPythonExceptionAsClrException();
            }
            return hash;
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
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PyObject pyObjectStr = new(CPythonAPI.PyObject_Str(this));
            return CPythonAPI.PyUnicode_AsUTF8(pyObjectStr);
        }
    }

    public T As<T>() => (T)As(typeof(T));

    /// <summary>
    /// Unpack a tuple of 2 elements into a KeyValuePair
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns></returns>
    public KeyValuePair<TKey, TValue> As<TKey, TValue>()
    {
        using (GIL.Acquire())
        {
            return PyObjectTypeConverter.ConvertToKeyValuePair<TKey, TValue>(this);
        }
    }

    internal object As(Type type)
    {
        using (GIL.Acquire())
        {
            return type switch
            {
                var t when t == typeof(PyObject) => Clone(),
                var t when t == typeof(bool) => CPythonAPI.IsPyTrue(this),
                var t when t == typeof(int) => CPythonAPI.PyLong_AsLong(this),
                var t when t == typeof(long) => CPythonAPI.PyLong_AsLongLong(this),
                var t when t == typeof(double) => CPythonAPI.PyFloat_AsDouble(this),
                var t when t == typeof(float) => (float)CPythonAPI.PyFloat_AsDouble(this),
                var t when t == typeof(string) => CPythonAPI.PyUnicode_AsUTF8(this),
                var t when t == typeof(BigInteger) => PyObjectTypeConverter.ConvertToBigInteger(this, t),
                var t when t == typeof(byte[]) => CPythonAPI.PyBytes_AsByteArray(this),
                var t when t.IsAssignableTo(typeof(ITuple)) => PyObjectTypeConverter.ConvertToTuple(this, t),
                var t when t.IsAssignableTo(typeof(IGeneratorIterator)) => PyObjectTypeConverter.ConvertToGeneratorIterator(this, t),
                var t when t.IsAssignableTo(typeof(ICoroutine)) => PyObjectTypeConverter.ConvertToCoroutine(this, t),
                var t => PyObjectTypeConverter.PyObjectToManagedType(this, t),
            };
        }
    }

    public static PyObject From<T>(T value)
    {
        switch (value)
        {
            case null: return None;
            case true: return True;
            case false: return False;
            case 0 or 0L or BigInteger { IsZero: true }: return Zero;
            case 1 or 1L or BigInteger { IsOne: true }: return One;
            case -1 or -1L:
            case BigInteger n when n == -1: return NegativeOne;
            default:
            {
                using (GIL.Acquire())
                {
                    return value switch
                    {
                        ICloneable pyObject => pyObject.Clone(),
                        int i => Create(CPythonAPI.PyLong_FromLong(i)),
                        long l => Create(CPythonAPI.PyLong_FromLongLong(l)),
                        double d => Create(CPythonAPI.PyFloat_FromDouble(d)),
                        float f => Create(CPythonAPI.PyFloat_FromDouble((double)f)),
                        string s => Create(CPythonAPI.AsPyUnicodeObject(s)),
                        byte[] bytes => PyObject.Create(CPythonAPI.PyBytes_FromByteSpan(bytes.AsSpan())),
                        IDictionary dictionary => PyObjectTypeConverter.ConvertFromDictionary(dictionary),
                        ITuple t => PyObjectTypeConverter.ConvertFromTuple(t),
                        ICollection l => PyObjectTypeConverter.ConvertFromList(l),
                        IEnumerable e => PyObjectTypeConverter.ConvertFromList(e),
                        BigInteger b => PyObjectTypeConverter.ConvertFromBigInteger(b),
                        _ => throw new InvalidCastException($"Cannot convert {value} to PyObject"),
                    };
                }
            }
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

    PyObject ICloneable.Clone() => Clone();
}
