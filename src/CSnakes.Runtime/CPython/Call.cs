using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static IntPtr Call(IntPtr callable, params IntPtr[] args)
    {
        if (callable == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(callable));
        }

        // TODO: Use vectorcall if possible https://docs.python.org/3/c-api/call.html#c.PyObject_Vectorcall

        // These options are used for efficiency. Don't create a tuple if its not required. 
        if (args.Length == 0)
        {
            return PyObject_CallNoArgs(callable);
        } else if (args.Length == 1)
        {
            return PyObject_CallOneArg(callable, args[0]);
        } else
        {
            var argsTuple = PackTuple(args);
            var result = PyObject_Call(callable, argsTuple, IntPtr.Zero);
            Py_DecRef(argsTuple);
            return result;
        }
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallNoArgs(IntPtr callable);

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallOneArg(IntPtr callable, IntPtr arg1);

    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Call(IntPtr callable, IntPtr args, IntPtr kwargs);
}
