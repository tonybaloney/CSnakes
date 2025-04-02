using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint ItemsStrIntern = IntPtr.Zero;
    public static bool IsPyMappingWithItems(PyObject p)
    {
        return PyMapping_Check(p) == 1 && PyObject_HasAttr(p, ItemsStrIntern) == 1;
    }

    /// <summary>
    /// Return 1 if the object provides the mapping protocol or supports slicing, and 0 otherwise. 
    /// Note that it returns 1 for Python classes with a __getitem__() method, since in general 
    /// it is impossible to determine what type of keys the class supports. This function always succeeds.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyMapping_Check(PyObject ob);

    /// <summary>
    /// Return the object from dictionary p which has a key `key`. 
    /// </summary>
    /// <param name="dict">Dictionary Object</param>
    /// <param name="key">Key Object</param>
    /// <exception cref="KeyNotFoundException">If the key is not found</exception>
    /// <returns>New reference.</returns>
    internal static nint PyMapping_GetItem(PyObject map, PyObject key)
    { 
        return PyObject_GetItem(map, key);
    }

    /// <summary>
    /// Insert val into the dictionary p with a key of key. 
    /// key must be hashable; if it isn’t, TypeError will be raised.  
    /// This function adds a reference to val and key if successful.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Return 0 on success or -1 on failure.</returns>
    internal static bool PyMapping_SetItem(PyObject dict, PyObject key, PyObject value)
    {
        return PyObject_SetItem(dict, key, value) == 0;
    }

    /// <summary>
    /// Get the items iterator for the mapping.
    /// </summary>
    /// <param name="dict">Object that implements the mapping protocol</param>
    /// <returns>New reference to the items().</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyMapping_Items(PyObject dict);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyMapping_Keys(PyObject dict);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyMapping_Values(PyObject dict);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyMapping_Size(PyObject dict);

    /// <summary>
    /// Return 1 if the mapping object has the key key and 0 otherwise. 
    /// This is equivalent to the Python expression key in o. This function always succeeds.
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyMapping_HasKey(PyObject dict, PyObject key);
}
