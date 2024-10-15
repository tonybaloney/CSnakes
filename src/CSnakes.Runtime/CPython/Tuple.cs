using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Set the Tuple Item at position `pos` to the object `o`.
    /// Adds a new reference to `o` if it was set successfully.
    /// </summary>
    /// <param name="ob">The tuple object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <param name="o">The new value</param>
    /// <returns>0 on success and -1 on failure</returns>
    internal static int SetItemInPyTuple(MPyOPtr ob, nint pos, MPyOPtr o)
    {
        int result = PyTuple_SetItem(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to tuple now. 
            Py_IncRef(o.DangerousGetHandle());
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
    internal static nint GetItemOfPyTuple(MPyOPtr ob, nint pos)
    {
        nint item = PyTuple_GetItem(ob, pos);
        if (item == IntPtr.Zero)
        {
            throw PythonObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRef(item);
        return item;
    }

    internal static bool IsPyTuple(MPyOPtr p) => IsInstance(p, _PyTupleType);

}
