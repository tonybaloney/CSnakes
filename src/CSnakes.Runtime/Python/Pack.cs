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
    internal static PyObject CreateTuple(Span<PyObject> items)
    {
        List<SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn> marshallers = new(items.Length);
        try
        {
            var handles = items.Length < 18 // .NET tuples are max 17 items. This is a performance optimization.
                ? stackalloc IntPtr[items.Length]
                : new IntPtr[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn m = default;
                m.FromManaged(items[i]);
                marshallers.Add(m);
                handles[i] = m.ToUnmanaged();
            }
            return PyObject.Create(CPythonAPI.PackTuple(handles));
        }
        finally
        {
            foreach (var m in marshallers)
            {
                m.Free();
            }
        }
    }

    internal static PyObject CreateList(Span<PyObject> items)
    {
        // As per Python/C API docs for `PyList_New`:
        //
        // > If len is greater than zero, the returned list object's items are set to `NULL`. Thus
        // > you cannot use abstract API functions such as `PySequence_SetItem()` or expose the
        // > object to Python code before setting all items to a real object with
        // > `PyList_SetItem()`.
        //
        // Source: https://docs.python.org/3/c-api/list.html#c.PyList_New

        nint list = 0;

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

            list = CPythonAPI.PyList_New(items.Length);

            var i = 0;
            foreach (var handle in handles)
            {
                int result = CPythonAPI.PyList_SetItemRaw(list, i++, handle);
                if (result == -1)
                {
                    throw PyObject.ThrowPythonExceptionAsClrException();
                }
            }

            return PyObject.Create(list);
        }
        catch
        {
            if (list != 0)
            {
                CPythonAPI.Py_DecRefRaw(list);
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
