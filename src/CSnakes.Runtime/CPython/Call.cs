using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    internal static IntPtr Call(PythonObject callable, Span<IntPtr> args)
    {
        // These options are used for efficiency. Don't create a tuple if its not required. 
        if (args.Length == 0)
        {
            return PyObject_CallNoArgs(callable);
        }
        else if (args.Length == 1 && PythonVersion.Major == 3 && PythonVersion.Minor > 10)
        {
            return PyObject_CallOneArg(callable, args[0]);
        }
        else if (args.Length > 1 && PythonVersion.Major == 3 && PythonVersion.Minor > 10)
        {
            fixed (IntPtr* argsPtr = args)
            {
                return PyObject_Vectorcall(callable, argsPtr, (nuint)args.Length, IntPtr.Zero);
            }
        }
        else
        {
            var argsTuple = PackTuple(args);
            var result = PyObject_Call(callable, argsTuple, IntPtr.Zero);
            Py_DecRef(argsTuple);
            return result;
        }
    }

    internal static IntPtr Call(PythonObject callable, Span<IntPtr> args, Span<string> kwnames, Span<IntPtr> kwvalues)
    {
        // These options are used for efficiency. Don't create a tuple if its not required. 
        if (false /* TODO: Implement vectorcall for kwargs*/ && 
            PythonVersion.Major == 3 && PythonVersion.Minor > 10)
        {
            fixed (IntPtr* argsPtr = args)
            {
                return PyObject_Vectorcall(callable, argsPtr, (nuint)args.Length, IntPtr.Zero);
            }
        }
        else
        {
            var argsTuple = PackTuple(args);
            var kwargsDict = PackDict(kwnames, kwvalues);
            var result = PyObject_Call(callable, argsTuple, kwargsDict);
            Py_DecRef(argsTuple);
            Py_DecRef(kwargsDict);
            return result;
        }
    }

    /// <summary>
    /// Call a callable with no arguments (3.9+)
    /// </summary>
    /// <param name="callable"></param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallNoArgs(PythonObject callable);

    /// <summary>
    /// Call a callable with one argument (3.11+)
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="arg1">The first argument</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallOneArg(PythonObject callable, IntPtr arg1);


    /// <summary>
    /// Call a callable with many arguments
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="args">A PyTuple of positional arguments</param>
    /// <param name="kwargs">A PyDict of keyword arguments</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Call(PythonObject callable, IntPtr args, IntPtr kwargs);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_Vectorcall(PythonObject callable, IntPtr* args, nuint nargsf, IntPtr kwnames);
}
