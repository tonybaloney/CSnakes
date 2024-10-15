using CSnakes.Runtime.Python;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

[DebuggerDisplay("MPyOPtr: {DangerousGetHandle()}")]
public class MPyOPtr : SafeHandle
{
    #region creation
    public static MPyOPtr Steal(pyoPtr pyObjectPtr) => new MPyOPtr(pyObjectPtr);
    #endregion

    #region SafeHandle logic
    protected MPyOPtr(pyoPtr pyObjectPtr, bool ownsHandle=true, bool incRef = false) : base(pyObjectPtr, ownsHandle)
    {
        if (pyObjectPtr == IntPtr.Zero)
        {
            // TODO: throw if there is an CPythonException otherwise a normal C# Exception... 
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }


        if (incRef) CAPI.Py_IncRef(pyObjectPtr);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (IsInvalid == false)
        {
            CAPI.Py_DecRef(handle);
            handle = IntPtr.Zero;
        }

        return true;
    }
    #endregion

    #region CSharp comparision support

    /// <summary>
    /// Are the objects the same instance, equivalent to the `is` operator in Python
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Is(MPyOPtr other)
    {
        return DangerousGetHandle() == other.DangerousGetHandle();
    }

    public override bool Equals(object? obj)
    {
        if (obj is MPyOPtr pyObj1)
        {
            if (Is(pyObj1))
                return true;
            return Compare(this, pyObj1, CAPI.RichComparisonType.Equal);
        }
        return base.Equals(obj);
    }

    public bool NotEquals(object? obj)
    {
        if (obj is MPyOPtr pyObj1)
        {
            if (Is(pyObj1))
                return false;
            return Compare(this, pyObj1, CAPI.RichComparisonType.NotEqual);
        }
        return !base.Equals(obj);
    }

    public static bool operator ==(MPyOPtr? left, MPyOPtr? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Equals(right),
        };
    }

    public static bool operator !=(MPyOPtr? left, MPyOPtr? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => true,
            (null, _) => true,
            (_, _) => left.NotEquals(right),
        };
    }

    public static bool operator <=(MPyOPtr? left, MPyOPtr? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, CAPI.RichComparisonType.LessThanEqual),
        };
    }

    public static bool operator >=(MPyOPtr? left, MPyOPtr? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, CAPI.RichComparisonType.GreaterThanEqual),
        };
    }

    public static bool operator <(MPyOPtr? left, MPyOPtr? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, CAPI.RichComparisonType.LessThan),
        };
    }

    public static bool operator >(MPyOPtr? left, MPyOPtr? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, CAPI.RichComparisonType.GreaterThan),
        };
    }

    private static bool Compare(MPyOPtr left, MPyOPtr right, CAPI.RichComparisonType type)
    {
        var result = CAPI.PyObject_RichCompareBool(left.DangerousGetHandle(), right.DangerousGetHandle(), type);
        if (result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }
    #endregion
}
