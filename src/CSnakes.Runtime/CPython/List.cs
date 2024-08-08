using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyListType = IntPtr.Zero;

    /// <summary>
    /// Create a new list of length `size`
    /// </summary>
    /// <param name="size">Size as ssize_t</param>
    /// <returns>A new reference to the list</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyList_New(nint size);

    /// <summary>
    /// Get the size of the list
    /// </summary>
    /// <param name="obj">PyList object</param>
    /// <returns>The size as ssize_t</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyList_Size(nint obj);

    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    internal static nint PyList_GetItem(nint obj, nint pos)
    {
        nint item = PyList_GetItem_(obj, pos);
        if (item == IntPtr.Zero)
        {
            PyErr_Clear();
            throw new IndexOutOfRangeException();
        }
        Py_IncRef(item);
        return item;
    }

    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>Borrowed reference to the list item</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_GetItem")]
    private static partial nint PyList_GetItem_(nint obj, nint pos);

    /// <summary>
    /// Set the item in a list and add a reference to it if successful. 
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <param name="o">The new value</param>
    /// <returns>-1 on failure and 0 on success</returns>
    internal static int PyList_SetItem(nint obj, nint pos, nint o)
    {
        int result = PyList_SetItem_(obj, pos, o);
        if (result != -1)
            Py_IncRef(o);
        return result;
    }

    /// <summary>
    /// Set an item in a list at position `pos` to the value of ``o``.
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The index position as ssize_t</param>
    /// <param name="o">The new value</param>
    /// <returns>-1 on failure and 0 on success</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_SetItem")]
    private static partial int PyList_SetItem_(nint obj, nint pos, nint o);

    internal static bool IsPyList(nint p)
    {
        return ((PyObjectStruct*)p)->Type() == PyListType;
    }
}
