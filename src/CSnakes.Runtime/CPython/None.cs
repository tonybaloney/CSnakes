using CSnakes.Runtime.Python;
namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static nint PyNone = IntPtr.Zero;

    /// <summary>
    /// Get the None object.
    /// </summary>
    /// <returns>A new reference to None. In newer versions of Python, None is immortal anyway.</returns>
    internal static nint GetNone()
    {
        if (PyNone == IntPtr.Zero)
        {
            throw new InvalidOperationException("Python is not initialized. You cannot call this method outside of a Python Environment context.");
        }
        Py_IncRefRaw(PyNone);
        return PyNone;
    }

    internal static bool IsNone(PyObject o)
    {
        return PyNone == o.DangerousGetHandle();
    }
}
