using CSnakes.Runtime.CPython;
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
        PyObject pyList = PyObject.Create(CPythonAPI.PyList_New(0)); // TODO: preallocate based on items.Length and use PyList_SetItem

        foreach (var item in items)
        {
            int result = CPythonAPI.PyList_Append(pyList, item);
            if (result == -1)
            {
                PyObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyList;
    }

    internal static PyObject CreateDictionary(IEnumerable<PyObject> keys,  IEnumerable<PyObject> values) {
        PyObject pyDict = PyObject.Create(CPythonAPI.PyDict_New());

        IEnumerator<PyObject> keyEnumerator = keys.GetEnumerator();
        IEnumerator<PyObject> valueEnumerator = values.GetEnumerator();

        while (keyEnumerator.MoveNext() && valueEnumerator.MoveNext())
        {
            int result = CPythonAPI.PyDict_SetItem(pyDict, keyEnumerator.Current, valueEnumerator.Current);
            if (result == -1)
            {
                PyObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyDict;
    }
}
