using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    /// <summary>
    /// Get the Type object for the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the type.</returns>
    internal static IntPtr GetType(PythonObject ob) {
        return PyObject_Type(ob.DangerousGetHandle());
    }


    /// <summary>
    /// Get the Type object for the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the type.</returns>
    internal static IntPtr GetTypeRaw(nint ob)
    {
        return PyObject_TypeRaw(ob);
    }

    /// <summary>
    /// When o is non-NULL, returns a type object corresponding to the object type of object o. 
    /// On failure, raises SystemError and returns NULL. 
    /// This is equivalent to the Python expression type(o). 
    /// This function creates a new strong reference to the return value.
    /// </summary>
    /// <param name="ob">The python object</param>
    /// <returns>A new reference to the Type object for the given Python object</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyObject_Type")]
    private static partial nint PyObject_TypeRaw(nint ob);

    internal static bool PyObject_IsInstance(PythonObject ob, IntPtr type)
    {
        int result = PyObject_IsInstance(ob.DangerousGetHandle(), type);
        if (result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }

    internal static IntPtr GetAttr(PythonObject ob, string name)
    {
        /* TODO: Consider interning/caching the name value */
        nint pyName = AsPyUnicodeObject(name);
        nint pyAttr = PyObject_GetAttr(ob.DangerousGetHandle(), pyName);
        Py_DecRef(pyName);
        return pyAttr;
    }

    internal static bool HasAttr(PythonObject ob, string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        int hasAttr = PyObject_HasAttr(ob, pyName);
        Py_DecRef(pyName);
        return hasAttr == 1;
    }

    /// <summary>
    /// Does the object ob have the attr `attr`?
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>1 on success, 0 if the attr does not exist</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_HasAttr(PythonObject ob, IntPtr attr);

    /// <summary>
    /// Get the iterator for the given object
    /// </summary>
    /// <param name="ob"></param>
    /// <returns>A new reference to the iterator</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetIter(PythonObject ob);

    /// <summary>
    /// Get the str(ob) form of the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the string representation</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_Str(PythonObject ob);

    /// <summary>
    /// Implements ob[key]
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="key"></param>
    /// <returns>A new reference to the object item if it exists, else NULL</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_GetItem(PythonObject ob, PythonObject key);

    /// <summary>
    /// Set ob[key] = value
    /// Returns -1 on error and sets exception.
    /// Does not steal a reference to value on success.
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_SetItem(PythonObject ob, PythonObject key, PythonObject value);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_Hash(PythonObject ob);

    internal static bool PyObject_RichCompare(PythonObject ob1, PythonObject ob2, RichComparisonType comparisonType)
    {
        int result = PyObject_RichCompareBool(ob1, ob2, comparisonType);
        if (result == -1)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_RichCompareBool(PythonObject ob1, PythonObject ob2, RichComparisonType comparisonType);
}
