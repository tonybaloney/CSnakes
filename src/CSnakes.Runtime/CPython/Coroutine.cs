using CSnakes.Runtime.Python;
using System.Threading.Tasks;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static bool IsPyCoroutine(PyObject p)
    {
        return HasAttr(p, "__await__");
    }

    internal class EventLoop : IDisposable
    {
        private readonly PyObject? loop = null;

        public EventLoop()
        {
            loop = NewEventLoopFactory!.Call();
        }

        public bool IsDisposed { get; private set; }

        private void Close()
        {
            if (loop is null)
            {
                throw new InvalidOperationException("Event loop not initialized");
            }
            using var close = loop.GetAttr("close");
            close?.Call();
        }

        public void Dispose()
        {
            Close();
            loop?.Dispose();
            IsDisposed = true;
        }

        public PyObject RunTaskUntilComplete(PyObject coroutine)
        {
            if (loop is null)
            {
                throw new InvalidOperationException("Event loop not initialized");
            }
            using var taskFunc = loop.GetAttr("create_task");
            using var task = taskFunc?.Call(coroutine) ?? PyObject.None;
            using var runUntilComplete = loop.GetAttr("run_until_complete");
            var result = runUntilComplete.Call(task);
            return result;
        }
    }

    [ThreadStatic] private static EventLoop? currentEventLoop = null;
    private static PyObject? AsyncioModule = null;
    private static PyObject? NewEventLoopFactory = null;

    internal static EventLoop WithEventLoop()
    {
        if (AsyncioModule is null)
        {
            throw new InvalidOperationException("Asyncio module not initialized");
        }
        if (currentEventLoop is null || currentEventLoop.IsDisposed)
        {
            currentEventLoop = new EventLoop();
        }
        return currentEventLoop!;
    }
}
