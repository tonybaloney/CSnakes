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
    internal static PyObject CreateTuple(IEnumerable<PyObject> items)
    {
        List<SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn> marshallers = new(items.Count());
        try
        {
            List<IntPtr> handles = new(items.Count());
            foreach (PyObject o in items)
            {
                SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn m = default;
                m.FromManaged(o);
                marshallers.Add(m);
                handles.Add(m.ToUnmanaged());
            }
            return PyObject.Create(CPythonAPI.PackTuple(handles.ToArray()))
                .RegisterDisposeHandler(() => {
                    foreach (var item in items)
                    {
                        item.Dispose();
                    }
                });
        }
        finally
        {
            foreach (var m in marshallers)
            {
                m.Free();
            }
        }
    }

    internal static PyObject CreateList(IEnumerable<PyObject> items)
    {
        PyObject pyList = PyObject.Create(CPythonAPI.PyList_New(0));

        foreach (var item in items)
        {
            int result = CPythonAPI.PyList_Append(pyList, item);
            if (result == -1)
            {
                PyObject.ThrowPythonExceptionAsClrException();
            }
        }

        return pyList.RegisterDisposeHandler(() => {
            foreach (var item in items)
            {
                item.Dispose();
            }
        });
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

        return pyDict.RegisterDisposeHandler(() =>
        {
            foreach (var key in keys)
            {
                key.Dispose();
            }
            foreach (var value in values)
            {
                value.Dispose();
            }
        });
    }
}
