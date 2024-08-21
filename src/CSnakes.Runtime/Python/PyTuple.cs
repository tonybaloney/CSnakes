using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;

internal static class PyTuple
{
    public static PyObject CreateTuple(IEnumerable<PyObject> items)
    {
        List<SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn> marshallers = new();
        try
        {
            List<IntPtr> handles = new();
            foreach (PyObject o in items)
            {
                SafeHandleMarshaller<PyObject>.ManagedToUnmanagedIn m = default;
                m.FromManaged(o);
                marshallers.Add(m);
                handles.Add(m.ToUnmanaged());
            }
            return PyObject.Create(CPythonAPI.PackTuple(handles.ToArray()));
        }
        finally
        {
            foreach (var m in marshallers)
            {
                m.Free();
            }
        }
    }
}
