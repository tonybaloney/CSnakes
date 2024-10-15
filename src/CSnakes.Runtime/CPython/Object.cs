using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    internal static bool IsInstance(MPyOPtr ob, pyoPtr type)
    {
        int result = PyObject_IsInstance(ob, type);
        if (result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }

    internal static pyoPtr GetAttr(MPyOPtr ob, string name) => GetAttr(ob.DangerousGetHandle(), name);
    internal static pyoPtr GetAttr(MPyOPtr ob, pyoPtr name) => GetAttr(ob.DangerousGetHandle(), name);
    internal static pyoPtr GetAttr(MPyOPtr ob, MPyOPtr name) => GetAttr(ob.DangerousGetHandle(), name.DangerousGetHandle());

    internal static bool HasAttr(MPyOPtr ob, string name) => HasAttr(ob.DangerousGetHandle(), name);
    internal static bool HasAttr(MPyOPtr ob, pyoPtr name) => HasAttr(ob.DangerousGetHandle(), name);
    internal static bool HasAttr(MPyOPtr ob, MPyOPtr name) => HasAttr(ob.DangerousGetHandle(), name.DangerousGetHandle());

    internal static bool RichComparePyObjects(MPyOPtr ob1, MPyOPtr ob2, RichComparisonType comparisonType)
    {
        int result = PyObject_RichCompareBool(ob1, ob2, comparisonType);
        if (result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }
}
