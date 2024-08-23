using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

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
}
