using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

internal static class PyTuple
{
    public static PyObject CreateTuple(params PyObject[] items) =>
        new(CPythonAPI.PackTuple(items.Select(item => item.DangerousGetHandle()).ToArray()));

    public static PyObject CreateTuple(IEnumerable<PyObject> items) =>
        new(CPythonAPI.PackTuple(items.Select(item => item.DangerousGetHandle()).ToArray()));
}
