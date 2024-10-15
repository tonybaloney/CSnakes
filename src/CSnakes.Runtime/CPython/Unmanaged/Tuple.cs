using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    protected static pyoPtr _PyTupleType = IntPtr.Zero;
    protected static pyoPtr _PyEmptyTuple = IntPtr.Zero;
    public static pyoPtr PtrToPyTupleType => _PyTupleType;

    public static pyoPtr GetPyEmptyTuple()
    {
        Py_IncRef(_PyEmptyTuple);
        return _PyEmptyTuple;
    }

    /// <summary>
    /// Create a PyTuple from the PyObject pointers in `items`.
    /// Function handles the reference increments to items.
    /// </summary>
    /// <param name="items">An array of pointers to PyObject</param>
    /// <returns>A new reference to the resulting tuple object.</returns>
    internal static pyoPtr PackTuple(Span<pyoPtr> items)
    {
        // This is a shortcut to a CPython optimization. Keep an empty tuple and reuse it.
        if (items.Length == 0)
            return GetPyEmptyTuple();

        nint tuple = PyTuple_New(items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            SetItemInPyTuple(tuple, i, items[i]);
        }
        return tuple;
    }


    /// <summary>
    /// Set the Tuple Item at position `pos` to the object `o`.
    /// Adds a new reference to `o` if it was set successfully.
    /// </summary>
    /// <param name="ob">The tuple object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <param name="o">The new value</param>
    /// <returns>0 on success and -1 on failure</returns>
    internal static int SetItemInPyTuple(pyoPtr ob, nint pos, pyoPtr o)
    {
        int result = PyTuple_SetItem(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to tuple now. 
            Py_IncRef(o);
        }
        return result;
    }

    /// <summary>
    /// Get an item at position `pos` from a PyTuple.
    /// </summary>
    /// <param name="ob">the Tuple object</param>
    /// <param name="pos">the index position as ssize_t</param>
    /// <returns>A new reference to the item.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    internal static nint GetItemOfPyTuple(pyoPtr ob, nint pos)
    {
        nint item = PyTuple_GetItem(ob, pos);
        if (item == IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRef(item);
        return item;
    }
}
