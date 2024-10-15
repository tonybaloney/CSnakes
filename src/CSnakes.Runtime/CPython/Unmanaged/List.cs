using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    protected static pyoPtr _PyListType = IntPtr.Zero;
    public static pyoPtr PtrToPyListType => _PyListType;


    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    internal static pyoPtr GetItemOfPyList(pyoPtr obj, nint pos)
    {
        nint item = PyList_GetItem(obj, pos);
        if (item == IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRef(item);
        return item;
    }


    internal static int SetItemInPyList(pyoPtr ob, nint pos, pyoPtr o)
    {
        int result = PyList_SetItem(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to list now.
            Py_IncRef(o);
        }
        return result;
    }
}
