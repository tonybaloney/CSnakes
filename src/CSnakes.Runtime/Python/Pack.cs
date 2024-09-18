using CSnakes.Runtime.CPython;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

/// <summary>
/// These methods are used internally to create a PyObject where the Dispose() call will dispose all items in 
/// the collection inside the same call stack. This avoids the .NET GC Finalizer thread from disposing the items
/// that were created and creating a GIL contention issue when other code is running.
/// </summary>
internal static class Pack
{
    internal static PyObject CreateTuple(Span<PyObject> items) =>
        PyObject.Create(CreateListOrTuple<TupleBuilder>(items));

    internal static PyObject CreateList(Span<PyObject> items) =>
        PyObject.Create(CreateListOrTuple<ListBuilder>(items));

    private interface IListOrTupleBuilder
    {
        static abstract nint New(nint size);
        static abstract int SetItemRaw(nint ob, nint pos, nint o);
    }

    private sealed class ListBuilder : IListOrTupleBuilder
    {
        // As per Python/C API docs for `PyList_New`:
        //
        // > If len is greater than zero, the returned list object's items are set to `NULL`. Thus
        // > you cannot use abstract API functions such as `PySequence_SetItem()` or expose the
        // > object to Python code before setting all items to a real object with
        // > `PyList_SetItem()`.
        //
        // Source: https://docs.python.org/3/c-api/list.html#c.PyList_New

        public static IntPtr New(IntPtr size) => CPythonAPI.PyList_New(size);
        public static int SetItemRaw(IntPtr ob, IntPtr pos, IntPtr o) => CPythonAPI.PyList_SetItemRaw(ob, pos, o);
    }

    private sealed class TupleBuilder : IListOrTupleBuilder
    {
        public static IntPtr New(IntPtr size) => size == 0 ? CPythonAPI.GetPyEmptyTuple() : CPythonAPI.PyTuple_New(size);
        public static int SetItemRaw(IntPtr ob, IntPtr pos, IntPtr o) => CPythonAPI.PyTuple_SetItemRaw(ob, pos, o);
    }

    private static nint CreateListOrTuple<TBuilder>(Span<PyObject> items)
        where TBuilder : IListOrTupleBuilder
    {
        nint obj = 0;

        List<SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn>? marshallers = null;

        try
        {
            var handles = items.Length <= 8
                ? stackalloc nint[items.Length]
                : new nint[items.Length];

            var uninitializedHandles = handles;
            foreach (var item in items)
            {
                SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn m = default;
                m.FromManaged(item);
                marshallers ??= new(items.Length);
                marshallers.Add(m);
                uninitializedHandles[0] = m.ToUnmanaged();
                uninitializedHandles = uninitializedHandles[1..];
            }

            Debug.Assert(uninitializedHandles.Length == 0);

            obj = TBuilder.New(items.Length);

            var i = 0;
            foreach (var handle in handles)
            {
                int result = TBuilder.SetItemRaw(obj, i++, handle);
                if (result == -1)
                {
                    throw PyObject.ThrowPythonExceptionAsClrException();
                }
            }

            return obj;
        }
        catch
        {
            if (obj != 0)
            {
                CPythonAPI.Py_DecRefRaw(obj);
            }

            throw;
        }
        finally
        {
            if (marshallers is not null)
            {
                foreach (var m in marshallers)
                {
                    m.Free();
                }
            }
        }
    }
}
