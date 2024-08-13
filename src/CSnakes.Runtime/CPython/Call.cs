using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    private static readonly Version extraCallArgsVersion = new(3, 9);

    internal static IntPtr Call(IntPtr callable, params IntPtr[] args)
    {
        if (callable == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(callable));
        }

        // TODO: Use vectorcall if possible https://docs.python.org/3/c-api/call.html#c.PyObject_Vectorcall

        // These options are used for efficiency. Don't create a tuple if its not required. 
        if (args.Length == 0 && PythonVersion >= extraCallArgsVersion)
        {
            return PyObject_CallNoArgs(callable);
        } else if (args.Length == 1 && PythonVersion >= extraCallArgsVersion)
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

    /// <summary>
    /// Call a callable with no arguments (3.9+)
    /// </summary>
    /// <param name="callable"></param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallNoArgs(IntPtr callable);

    /// <summary>
    /// Call a callable with one argument (3.9+)
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="arg1">The first argument</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallOneArg(IntPtr callable, IntPtr arg1);

    /// <summary>
    /// Call a callable with many arguments
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="args">A PyTuple of positional arguments</param>
    /// <param name="kwargs">A PyDict of keyword arguments</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Call(IntPtr callable, IntPtr args, IntPtr kwargs);
}
