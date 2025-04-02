using CSnakes.Runtime.Python;
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
    internal static partial nint PyList_Size(PyObject obj);

    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    internal static nint PyList_GetItem(PyObject obj, nint pos)
    {
        nint item = PyList_GetItem_(obj, pos);
        if (item == IntPtr.Zero)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRefRaw(item);
        return item;
    }

    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>Borrowed reference to the list item</returns>
    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_GetItem")]
    private static partial nint PyList_GetItem_(PyObject obj, nint pos);

    internal static int PyList_SetItemRaw(nint ob, nint pos, nint o)
    {
        int result = PyList_SetItem_(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to list now.
            Py_IncRefRaw(o);
        }
        return result;
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_SetItem")]
    internal static partial int PyList_SetItem_(nint obj, nint pos, nint o);

    internal static bool IsPyList(PyObject p)
    {
        return PyObject_IsInstance(p, PyListType);
    }
}
