using CSnakes.Runtime.Locators;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    private static nint PyInterpreterState = 0;

    /// <summary>
    /// Ensure (hold) GIL and return a GIL state handle (ptr)
    /// Handles are not memory managed by Python. Do not free them.
    /// </summary>
    /// <returns>A GIL state handle</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyGILState_Ensure();

    /// <summary>
    /// Release the GIL with the given state handle
    /// </summary>
    /// <param name="state">GIL State handle</param>
    [LibraryImport(PythonLibraryName)]
    internal static partial void PyGILState_Release(nint state);

    /// <summary>
    /// Check the GIL is held by the current thread
    /// </summary>
    /// <returns>1 if held, 0 if not.</returns>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyGILState_Check();

    internal static int GetNativeThreadId()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetCurrentThreadId();
        else
            return -1;
    }

    [LibraryImport("kernel32.dll")]
    private static partial int GetCurrentThreadId();

    [LibraryImport(PythonLibraryName)]
    internal static partial void PyEval_ReleaseLock();

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyEval_SaveThread();

    [LibraryImport(PythonLibraryName)]
    internal static partial void PyEval_RestoreThread(nint tstate);

    internal static nint PyThreadState_New()
    {
        Debug.Assert(PyInterpreterState != 0);
        return PyThreadState_New_(PyInterpreterState);
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyThreadState_New")]
    private static partial nint PyThreadState_New_(nint istate);

    [LibraryImport(PythonLibraryName)]
    private static partial nint PyInterpreterState_Get();
}
