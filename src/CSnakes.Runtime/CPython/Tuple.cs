using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    private static nint PyTupleType = IntPtr.Zero;
    private static nint PyEmptyTuple = IntPtr.Zero;

    /// <summary>
    /// Create a PyTuple from the PyObject pointers in `items`.
    /// Function handles the reference increments to items.
    /// </summary>
    /// <param name="items">An array of pointers to PyObject</param>
    /// <returns>A new reference to the resulting tuple object.</returns>
    internal static nint PackTuple(Span<IntPtr> items)
    {
        // This is a shortcut to a CPython optimization. Keep an empty tuple and reuse it.
        if (items.Length == 0)
        {
            Py_IncRefRaw(PyEmptyTuple);
            return PyEmptyTuple;
        }

        nint tuple = PyTuple_New(items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            PyTuple_SetItemRaw(tuple, i, items[i]);
        }
        return tuple;
    }

    /// <summary>
    /// Create a new tuple of size `size` as ssize_t
    /// </summary>
    /// <param name="size"></param>
    /// <returns>A new reference to the tuple object, or NULL on failure.</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_New(nint size);

    /// <summary>
    /// Set the Tuple Item at position `pos` to the object `o`.
    /// Adds a new reference to `o` if it was set successfully.
    /// </summary>
    /// <param name="ob">The tuple object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <param name="o">The new value</param>
    /// <returns>0 on success and -1 on failure</returns>
    internal static int PyTuple_SetItemRaw(nint ob, nint pos, nint o)
    {
        int result = PyTuple_SetItem_(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to tuple now. 
            Py_IncRefRaw(o);
        }
        return result;
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyTuple_SetItem")]
    private static partial int PyTuple_SetItem_(nint ob, nint pos, nint o);

    /// <summary>
    /// Get an item at position `pos` from a PyTuple.
    /// </summary>
    /// <param name="ob">the Tuple object</param>
    /// <param name="pos">the index position as ssize_t</param>
    /// <returns>A new reference to the item.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    internal static nint PyTuple_GetItemWithNewRef(PyObject ob, nint pos)
    {
        nint item = PyTuple_GetItem(ob, pos);
        if (item == IntPtr.Zero)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRefRaw(item);
        return item;
    }

    internal static nint PyTuple_GetItemWithNewRefRaw(nint ob, nint pos)
    {
        nint item = PyTuple_GetItemRaw(ob, pos);
        if (item == IntPtr.Zero)
        {
            throw PyObject.ThrowPythonExceptionAsClrException();
        }
        Py_IncRefRaw(item);
        return item;
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyTuple_GetItem")]
    private static partial nint PyTuple_GetItem(PyObject ob, nint pos);

    [LibraryImport(PythonLibraryName, EntryPoint = "PyTuple_GetItem")]
    private static partial nint PyTuple_GetItemRaw(nint ob, nint pos);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyTuple_Size(PyObject p);

    internal static bool IsPyTuple(PyObject p)
    {
        return PyObject_IsInstance(p, PyTupleType);
    }
}
