using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Repr(IntPtr ob);

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_DecRef(nint ob);

    [LibraryImport(PythonLibraryName)]
    internal static partial void Py_IncRef(nint ob);

    /// <summary>
    /// Get the Type object for the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the type.</returns>
    internal static IntPtr GetType(IntPtr ob) {
        nint type = ((PyObjectStruct*)ob)->Type();
        Py_IncRef(type);
        return type;
    }

    internal static IntPtr GetAttr(IntPtr ob, string name)
    {
        /* TODO: Consider interning/caching the name value */
        nint pyName = AsPyUnicodeObject(name);
        nint pyAttr = PyObject_GetAttr(ob, pyName);
        Py_DecRef(pyName);
        return pyAttr;
    }

    /// <summary>
    /// Get the attribute with name `attr` from the object `ob`
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>A new reference to the attribute</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetAttr(IntPtr ob, IntPtr attr);

    /// <summary>
    /// Does the object ob have the attr `attr`?
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <param name="attr">The attribute as a PyUnicode object</param>
    /// <returns>1 on success, 0 if the attr does not exist</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyObject_HasAttr(IntPtr ob, IntPtr attr);

    /// <summary>
    /// Get the iterator for the given object
    /// </summary>
    /// <param name="ob"></param>
    /// <returns>A new reference to the iterator</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_GetIter(IntPtr ob);

    /// <summary>
    /// Get the str(ob) form of the object
    /// </summary>
    /// <param name="ob">The Python object</param>
    /// <returns>A new reference to the string representation</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_Str(nint ob);
}
