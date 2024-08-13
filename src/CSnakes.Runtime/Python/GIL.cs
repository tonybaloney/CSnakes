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
            gilState = CPythonAPI.PyGILState_Ensure();
        }

        public void Dispose()
        {
            CPythonAPI.PyGILState_Release(gilState);
        }
    }

    public static IDisposable Acquire()
    {
        // TODO: Decide if we want access to the `ILogger` here to pass to the `PyGilState` constructor
        return new PyGilState();
    }
}
