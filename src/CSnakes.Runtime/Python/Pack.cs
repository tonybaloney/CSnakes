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
    internal static unsafe PyObject CreateTuple(Span<PyObject> items) =>
        PyObject.Create(CreateListOrTuple(items, &CPythonAPI.PyTuple_New, &CPythonAPI.PyTuple_SetItemRaw));

    internal static unsafe PyObject CreateList(Span<PyObject> items) =>
        PyObject.Create(CreateListOrTuple(items, &CPythonAPI.PyList_New, &CPythonAPI.PyList_SetItemRaw));

    public static unsafe nint CreateListOrTuple(Span<PyObject> items,
                                                delegate* <nint, nint> newObject,
                                                delegate* <nint, nint, nint, int> setItemRaw)
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

            obj = newObject(items.Length);

            var i = 0;
            foreach (var handle in handles)
            {
                int result = setItemRaw(obj, i++, handle);
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
