using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python.Interns;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

[DebuggerDisplay("PythonObject: repr={GetRepr()}, type={GetPythonType().ToString()}")]
public class PythonObject : SafeHandle, ICloneable
{
    protected PythonObject(IntPtr pyObject, bool ownsHandle = true) : base(pyObject, ownsHandle)
    {
        if (pyObject == IntPtr.Zero)
        {
            throw ThrowPythonExceptionAsClrException();
        }
    }

    internal static PythonObject Create(IntPtr ptr)
    {
        if (None.DangerousGetHandle() == ptr)
            return None;
        return new PythonObject(ptr);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (IsInvalid)
            return true;
        if (!CAPI.IsInitialized)
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
                CAPI.Py_DecRef(handle);
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
            if (!CAPI.IsPyErrOccurred())
            {
                return new InvalidDataException("An error occurred in Python, but no exception was set.");
            }
            CAPI.PyErr_Fetch(out nint excType, out nint excValue, out nint excTraceback);

            if (excType == 0)
            {
                return new InvalidDataException("An error occurred in Python, but no exception was set.");
            }

            using var pyExceptionType = Create(excType);
            PythonObject? pyExceptionTraceback = excTraceback == IntPtr.Zero ? null : new PythonObject(excTraceback);
            PythonObject? pyException = excValue == IntPtr.Zero ? null : Create(excValue);

            // TODO: Consider adding __qualname__ as well for module exceptions that aren't builtins
            var pyExceptionTypeStr = pyExceptionType.GetAttr("__name__").ToString();
            CAPI.PyErr_Clear();

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
        if (!CAPI.IsInitialized)
        {
            throw new InvalidOperationException("Python is not initialized. You cannot call this method outside of a Python Environment context.");
        }
    }

    /// <summary>
    /// Get the type for the object.
    /// </summary>
    /// <returns>A new reference to the type field.</returns>
    public virtual PythonObject GetPythonType()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PythonObject(CAPI.GetType(this));
        }
    }

    /// <summary>
    /// Get the attribute of the object with name. This is equivalent to obj.name in Python.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Attribute object (new ref)</returns>
    public virtual PythonObject GetAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(CAPI.GetAttr(this, name));
        }
    }

    public virtual bool HasAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return CAPI.HasAttr(this, name);
        }
    }


    internal virtual PythonObject GetIter()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(CAPI.PyObject_GetIter(this));
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
            using PythonObject reprStr = new PythonObject(CAPI.PyObject_Repr(this.DangerousGetHandle()));
            return CAPI.PyUnicode_AsUTF8(reprStr);
        }
    }

    /// <summary>
    /// Is the Python object None?
    /// </summary>
    /// <returns>true if None, else false</returns>
    public virtual bool IsNone() => CAPI.IsNone(this);

    /// <summary>
    /// Are the objects the same instance, equivalent to the `is` operator in Python
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Is(PythonObject other)
    {
        return DangerousGetHandle() == other.DangerousGetHandle();
    }

    public override bool Equals(object? obj)
    {
        if (obj is PythonObject pyObj1)
        {
            if (Is(pyObj1))
                return true;
            return Compare(this, pyObj1, CAPI.RichComparisonType.Equal);
        }
        return base.Equals(obj);
    }

    public bool NotEquals(object? obj)
    {
        if (obj is PythonObject pyObj1)
        {
            if (Is(pyObj1))
                return false;
            return Compare(this, pyObj1, CAPI.RichComparisonType.NotEqual);
        }
        return !base.Equals(obj);
    }

    public static bool operator ==(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Equals(right),
        };
    }

    public static bool operator !=(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => true,
            (null, _) => true,
            (_, _) => left.NotEquals(right),
        };
    }

    public static bool operator <=(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, CAPI.RichComparisonType.LessThanEqual),
        };
    }

    public static bool operator >=(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, CAPI.RichComparisonType.GreaterThanEqual),
        };
    }

    public static bool operator <(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, CAPI.RichComparisonType.LessThan),
        };
    }

    public static bool operator >(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, CAPI.RichComparisonType.GreaterThan),
        };
    }

    private static bool Compare(PythonObject left, PythonObject right, CAPI.RichComparisonType type)
    {
        using (GIL.Acquire())
        {
            return CAPI.PyObject_RichCompare(left, right, type);
        }
    }

    public override int GetHashCode()
    {
        using (GIL.Acquire())
        {
            int hash = CAPI.PyObject_Hash(this);
            if (hash == -1)
            {
                throw ThrowPythonExceptionAsClrException();
            }
            return hash;
        }
    }

    public static PythonObject None { get; } = new PyNoneObject();
    public static PythonObject True { get; } = new PyTrueObject();
    public static PythonObject False { get; } = new PyFalseObject();


    /// <summary>
    /// Call the object. Equivalent to (__call__)(args)
    /// All arguments are treated as positional.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The resulting object, or NULL on error.</returns>
    public PythonObject Call(params PythonObject[] args)
    {
        return CallWithArgs(args);
    }

    public PythonObject CallWithArgs(PythonObject[]? args = null)
    {
        RaiseOnPythonNotInitialized();

        // Don't do any marshalling if there aren't any arguments. 
        if (args is null || args.Length == 0)
        {
            using (GIL.Acquire())
            {
                return Create(CAPI.PyObject_CallNoArgs(this));
            }
        }

        args ??= [];
        var marshallers = new SafeHandleMarshaller<PythonObject>.ManagedToUnmanagedIn[args.Length];
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
                return Create(CAPI.Call(this, argHandles));
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

    public PythonObject CallWithKeywordArguments(PythonObject[]? args = null, string[]? kwnames = null, PythonObject[]? kwvalues = null)
    {
        if (kwnames is null)
            return CallWithArgs(args);
        if (kwvalues is null || kwnames.Length != kwvalues.Length)
            throw new ArgumentException("kwnames and kwvalues must be the same length.");
        RaiseOnPythonNotInitialized();
        args ??= [];

        var argMarshallers = new SafeHandleMarshaller<PythonObject>.ManagedToUnmanagedIn[args.Length];
        var kwargMarshallers = new SafeHandleMarshaller<PythonObject>.ManagedToUnmanagedIn[kwvalues.Length];
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
                return Create(CAPI.Call(this, argHandles, kwnames, kwargHandles));
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

    public PythonObject CallWithKeywordArguments(PythonObject[]? args = null, string[]? kwnames = null, PythonObject[]? kwvalues = null, IReadOnlyDictionary<string, PythonObject>? kwargs = null)
    {
        // No keyword parameters supplied
        if (kwnames is null && kwargs is null)
            return CallWithArgs(args);
        // Keyword args are empty and kwargs is empty. 
        if (kwnames is not null && kwnames.Length == 0 && (kwargs is null || kwargs.Count == 0))
            return CallWithArgs(args);

        MergeKeywordArguments(kwnames ?? [], kwvalues ?? [], kwargs, out string[] combinedKwnames, out PythonObject[] combinedKwvalues);
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
            using PythonObject pyObjectStr = new(CAPI.PyObject_Str(this));
            return CAPI.PyUnicode_AsUTF8(pyObjectStr);
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
                var t when t == typeof(PythonObject) => Clone(),
                var t when t == typeof(bool) => CAPI.IsPyTrue(this),
                var t when t == typeof(int) => CAPI.PyLong_AsLong(this),
                var t when t == typeof(long) => CAPI.PyLong_AsLongLong(this),
                var t when t == typeof(double) => CAPI.PyFloat_AsDouble(this),
                var t when t == typeof(float) => (float)CAPI.PyFloat_AsDouble(this),
                var t when t == typeof(string) => CAPI.PyUnicode_AsUTF8(this),
                var t when t == typeof(BigInteger) => PyObjectTypeConverter.ConvertToBigInteger(this, t),
                var t when t == typeof(byte[]) => CAPI.PyBytes_AsByteArray(this),
                var t when t.IsAssignableTo(typeof(ITuple)) => PyObjectTypeConverter.ConvertToTuple(this, t),
                var t when t.IsAssignableTo(typeof(IGeneratorIterator)) => PyObjectTypeConverter.ConvertToGeneratorIterator(this, t),
                var t => PyObjectTypeConverter.PyObjectToManagedType(this, t),
            };
        }
    }

    public static PythonObject From<T>(T value)
    {
        using (GIL.Acquire())
        {
            if (value is null)
                return None;

            return value switch
            {
                ICloneable pyObject => pyObject.Clone(),
                bool b => b ? True : False,
                int i => Create(CAPI.PyLong_FromLong(i)),
                long l => Create(CAPI.PyLong_FromLongLong(l)),
                double d => Create(CAPI.PyFloat_FromDouble(d)),
                float f => Create(CAPI.PyFloat_FromDouble((double)f)),
                string s => Create(CAPI.AsPyUnicodeObject(s)),
                byte[] bytes => PythonObject.Create(CAPI.PyBytes_FromByteSpan(bytes.AsSpan())),
                IDictionary dictionary => PyObjectTypeConverter.ConvertFromDictionary(dictionary),
                ITuple t => PyObjectTypeConverter.ConvertFromTuple(t),
                ICollection l => PyObjectTypeConverter.ConvertFromList(l),
                IEnumerable e => PyObjectTypeConverter.ConvertFromList(e),
                BigInteger b => PyObjectTypeConverter.ConvertFromBigInteger(b),
                _ => throw new InvalidCastException($"Cannot convert {value} to PyObject"),
            };
        }
    }

    internal virtual PythonObject Clone()
    {
        CAPI.Py_IncRef(handle);
        return new PythonObject(handle);
    }

    private static void MergeKeywordArguments(string[] kwnames, PythonObject[] kwvalues, IReadOnlyDictionary<string, PythonObject>? kwargs, out string[] combinedKwnames, out PythonObject[] combinedKwvalues)
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
        var newKwvalues = new List<PythonObject>(kwvalues);

        // The order must be the same as we're not submitting these in a mapping, but a parallel array.
        foreach (var (key, value) in kwargs)
        {
            newKwnames.Add(key);
            newKwvalues.Add(value);
        }

        combinedKwnames = [.. newKwnames];
        combinedKwvalues = [.. newKwvalues];
    }

    PythonObject ICloneable.Clone() => Clone();
}
