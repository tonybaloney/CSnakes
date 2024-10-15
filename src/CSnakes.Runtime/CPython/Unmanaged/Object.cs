using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    internal static pyoPtr GetAttr(pyoPtr ob, string name)
    {
        /* TODO: Consider interning/caching the name value */
        nint pyName = AsPyUnicodeObject(name);
        nint pyAttr = PyObject_GetAttr(ob, pyName);
        Py_DecRef(pyName);
        return pyAttr;
    }

    internal static pyoPtr GetAttr(pyoPtr ob, pyoPtr name)
    {
        /* TODO: Consider interning/caching the name value */
        nint pyAttr = PyObject_GetAttr(ob, name);
        return pyAttr;
    }

    internal static bool HasAttr(pyoPtr ob, string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        int hasAttr = PyObject_HasAttr(ob, pyName);
        Py_DecRef(pyName);
        return hasAttr == 1;
    }

    internal static bool HasAttr(pyoPtr ob, pyoPtr name)
    {
        int hasAttr = PyObject_HasAttr(ob, name);
        return hasAttr == 1;
    }


    internal static bool RichComparePyObjects(pyoPtr ob1, pyoPtr ob2, RichComparisonType comparisonType)
    {
        int result = PyObject_RichCompareBool(ob1, ob2, comparisonType);
        if (result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }
}
