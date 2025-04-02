using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal enum RichComparisonType : int
    {
        LessThan = 0,
        LessThanEqual = 1,
        Equal = 2,
        NotEqual = 3,
        GreaterThan = 4,
        GreaterThanEqual = 5
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Repr(PyObject ob);

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_DecRef(PyObject ob);

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_IncRef(PyObject ob);

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_DecRef")]
    internal static partial void Py_DecRefRaw(nint ob);

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_IncRef")]
    internal static partial void Py_IncRefRaw(nint ob);

    /// <summary>
    /// Get the Type object for the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the type.</returns>
    internal static IntPtr GetType(PyObject ob) {
        return PyObject_Type(ob);
    }

    /// <summary>
    /// When o is non-NULL, returns a type object corresponding to the object type of object o. 
    /// On failure, raises SystemError and returns NULL. 
    /// This is equivalent to the Python expression type(o). 
    /// This function creates a new strong reference to the return value.
    /// </summary>
    /// <param name="ob">The python object</param>
    /// <returns>A new reference to the Type object for the given Python object</returns>
    [LibraryImport(PythonLibraryName)]
    private static partial nint PyObject_Type(PyObject ob);

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

    internal static bool PyObject_IsInstance(PyObject ob, IntPtr type)
    {
        int result = PyObject_IsInstance_(ob, type);
        if (result == -1)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }

    /// <summary>
    /// Return 1 if inst is an instance of the class cls or a subclass of cls, or 0 if not. 
    /// On error, returns -1 and sets an exception.
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="type">The Python type object</param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyObject_IsInstance")]
    private static partial int PyObject_IsInstance_(PyObject ob, IntPtr type);

    internal static IntPtr GetAttr(PyObject ob, string name)
    {
        /* TODO: Consider interning/caching the name value */
        nint pyName = AsPyUnicodeObject(name);
        nint pyAttr = PyObject_GetAttr(ob, pyName);
        Py_DecRefRaw(pyName);
        return pyAttr;
    }

    internal static bool HasAttr(PyObject ob, string name)
    {
        nint pyName = AsPyUnicodeObject(name);
        int hasAttr = PyObject_HasAttr(ob, pyName);
        Py_DecRefRaw(pyName);
        return hasAttr == 1;
    }

    internal static void SetAttr(PyObject ob, string name, PyObject value)
    {
        nint pyName = AsPyUnicodeObject(name);
        int result = PyObject_SetAttr(ob, pyName, value);
        Py_DecRefRaw(pyName);
        if (result == -1)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
    }

    /// <summary>
    /// Get the attribute with name `attr` from the object `ob`
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>A new reference to the attribute</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetAttr(PyObject ob, IntPtr attr);

    [LibraryImport(PythonLibraryName, EntryPoint = "PyObject_GetAttr")]
    private static partial IntPtr PyObject_GetAttrRaw(IntPtr ob, IntPtr attr);

    /// <summary>
    /// Does the object ob have the attr `attr`?
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>1 on success, 0 if the attr does not exist</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_HasAttr(PyObject ob, IntPtr attr);

    [LibraryImport(PythonLibraryName)]
    private static partial int PyObject_SetAttr(PyObject ob, IntPtr attr, PyObject value);

    /// <summary>
    /// Get the iterator for the given object
    /// </summary>
    /// <param name="ob"></param>
    /// <returns>A new reference to the iterator</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetIter(PyObject ob);

    /// <summary>
    /// Get the str(ob) form of the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the string representation</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_Str(PyObject ob);

    /// <summary>
    /// Implements ob[key]
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="key"></param>
    /// <returns>A new reference to the object item if it exists, else NULL</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_GetItem(PyObject ob, PyObject key);

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
    internal static partial int PyObject_SetItem(PyObject ob, PyObject key, PyObject value);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_Hash(PyObject ob);

    internal static bool PyObject_RichCompare(PyObject ob1, PyObject ob2, RichComparisonType comparisonType)
    {
        int result = PyObject_RichCompareBool(ob1, ob2, comparisonType);
        if (result == -1)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        return result == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_RichCompareBool(PyObject ob1, PyObject ob2, RichComparisonType comparisonType);
}
