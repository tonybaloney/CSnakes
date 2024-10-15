using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    private static nint PyListType = IntPtr.Zero;

    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    internal static nint PyList_GetItem(PythonObject obj, nint pos)
    {
        nint item = PyList_GetItem_(obj, pos);
        if (item == IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
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
    private static partial nint PyList_GetItem_(PythonObject obj, nint pos);

    internal static int PyList_SetItemRaw(nint ob, nint pos, nint o)
    {
        int result = PyList_SetItem_(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to list now.
            Py_IncRef(o);
        }
        return result;
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_SetItem")]
    internal static partial int PyList_SetItem_(nint obj, nint pos, nint o);

    internal static bool IsPyList(PythonObject p)
    {
        return PyObject_IsInstance(p, PyListType);
    }
}
