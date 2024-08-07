using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class GIL
{
    // TODO: Handle environments without the GIL (although GIL state ensure becomes a noop)
    internal class PyGilState : IDisposable
    {
        private readonly IntPtr _state;

        public PyGilState()
        {
            _state = CPythonAPI.PyGILState_Ensure();
        }

        public void Dispose()
        {
            CPythonAPI.PyGILState_Release(_state);
        }
    }

    public static IDisposable Acquire()
    {
        return new PyGilState();
    }
}
