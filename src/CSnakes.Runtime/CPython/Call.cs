using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static IntPtr Call(PyObject callable, ReadOnlySpan<IntPtr> args)
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
            Py_DecRefRaw(argsTuple);
            return result;
        }
    }

    internal static IntPtr Call(PyObject callable, ReadOnlySpan<IntPtr> args, ReadOnlySpan<IntPtr> kwnames, ReadOnlySpan<IntPtr> kwvalues)
    {
        // Vectorcall takes the keyword values after the positional ones and their names as a tuple: no tuple of the
        // arguments and no dictionary of the keywords, which a function made for vectorcall would only unpack again.
        if (args.Length + kwvalues.Length <= MaxVectorcallArgs && PythonVersion.Major == 3 && PythonVersion.Minor > 10
            && AreVectorcallKeywords(kwnames))
        {
            Span<IntPtr> all = stackalloc IntPtr[MaxVectorcallArgs];
            args.CopyTo(all);
            kwvalues.CopyTo(all[args.Length..]);
            var kwnamesTuple = PackTuple(kwnames);
            try
            {
                fixed (IntPtr* argsPtr = all)
                {
                    return PyObject_Vectorcall(callable, argsPtr, (nuint)args.Length, kwnamesTuple);
                }
            }
            finally
            {
                Py_DecRefRaw(kwnamesTuple);
            }
        }
        else
        {
            var argsTuple = PackTuple(args);
            var kwargsDict = PackDict(kwnames, kwvalues);
            var result = PyObject_Call(callable, argsTuple, kwargsDict);
            Py_DecRefRaw(argsTuple);
            Py_DecRefRaw(kwargsDict);
            return result;
        }
    }

    /// <summary>
    /// The most arguments, positional and keyword ones together, that a call with keywords passes by vectorcall; more go
    /// in a tuple and a dictionary, as each name is compared with the others.
    /// </summary>
    private const int MaxVectorcallArgs = 16;

    /// <summary>
    /// Whether vectorcall takes these keyword names: it needs them to be of type <c>str</c> exactly, not a subclass, and
    /// each name once. Anything else goes in a dictionary, which says what's wrong as Python does, or keeps the last of
    /// two values of one name.
    /// </summary>
    private static bool AreVectorcallKeywords(ReadOnlySpan<IntPtr> kwnames)
    {
        for (int i = 0; i < kwnames.Length; i++)
        {
            var type = GetTypeRaw(kwnames[i]);
            Py_DecRefRaw(type);
            if (type != PyUnicodeType)
                return false;
            for (int j = 0; j < i; j++)
            {
                if (kwnames[i] == kwnames[j] || PyUnicode_Compare(kwnames[i], kwnames[j]) == 0)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Call a callable with no arguments (3.9+)
    /// </summary>
    /// <param name="callable"></param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallNoArgs(PyObject callable);

    /// <summary>
    /// Call a callable with one argument (3.11+)
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="arg1">The first argument</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_CallOneArg(PyObject callable, IntPtr arg1);


    /// <summary>
    /// Call a callable with many arguments
    /// </summary>
    /// <param name="callable">Callable object</param>
    /// <param name="args">A PyTuple of positional arguments</param>
    /// <param name="kwargs">A PyDict of keyword arguments</param>
    /// <returns>A new reference to the result, or null on failure</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial IntPtr PyObject_Call(PyObject callable, IntPtr args, IntPtr kwargs);

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyObject_Vectorcall(PyObject callable, IntPtr* args, nuint nargsf, IntPtr kwnames);
}
