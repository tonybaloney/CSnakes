using CSnakes.Runtime.CPython;
using System.Diagnostics;

namespace CSnakes.Runtime.Python;

public static class GIL
{
    [ThreadStatic] private static PyGilState? currentState;

    internal class PyGilState : IDisposable
    {
        private nint gilState;
        private int recursionCount;

        public PyGilState()
        {
            Debug.Assert(CPythonAPI.IsInitialized);
            gilState = CPythonAPI.PyGILState_Ensure();
            recursionCount = 1;
        }

        public void Ensure()
        {
            if (recursionCount == 0)
            {
                gilState = CPythonAPI.PyGILState_Ensure();
            }
            recursionCount++;
        }

        public void Dispose()
        {
            recursionCount--;
            if (recursionCount > 0)
            {
                return;
            }
            CPythonAPI.PyGILState_Release(gilState);
        }
    }

    public static IDisposable Acquire()
    {
        if (currentState == null)
        {
            currentState = new PyGilState();
        }
        else
        {
            currentState.Ensure();
        }
        return currentState;
    }
}
