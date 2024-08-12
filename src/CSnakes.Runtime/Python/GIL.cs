using CSnakes.Runtime.CPython;
using System.Diagnostics;

namespace CSnakes.Runtime.Python;

public static class GIL
{
    internal class PyGilState : IDisposable
    {
        private readonly nint gilState;

        public PyGilState()
        {
            Debug.Assert(CPythonAPI.IsInitialized);
#if DEBUG
            if (CPythonAPI.PyGILState_Check() == 1)
            {
                Debug.WriteLine($"GIL already acquired for thread {Environment.CurrentManagedThreadId}");
            }
#endif
            gilState = CPythonAPI.PyGILState_Ensure();
            Debug.WriteLine($"GIL acquired for thread {Environment.CurrentManagedThreadId} ({CPythonAPI.GetNativeThreadId()})");
        }

        public void Dispose()
        {
            Debug.WriteLine($"Releasing GIL for thread {Thread.CurrentThread.ManagedThreadId}");
            CPythonAPI.Py_MakePendingCalls();
            CPythonAPI.PyGILState_Release(gilState);
        }
    }

    public static IDisposable Acquire()
    {
        return new PyGilState();
    }
}
