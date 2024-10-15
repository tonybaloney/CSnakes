namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    internal static nint Call(pyoPtr callable, Span<pyoPtr> args)
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

    internal static IntPtr Call(pyoPtr callable, Span<pyoPtr> args, Span<string> kwnames, Span<pyoPtr> kwvalues)
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
}
