using CSnakes.Runtime.Python;
using System.Collections.Concurrent;

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
            if (NewEventLoopFactory is null)
            {
                throw new InvalidOperationException("NewEventLoopFactory not initialized");
            }
            loop = NewEventLoopFactory.Call();
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
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            Close();
            loop?.Dispose();
            IsDisposed = true;
        }

        public PyObject RunTaskUntilComplete(PyObject coroutine, CancellationToken? cancellationToken = null)
        {
            if (loop is null)
            {
                throw new InvalidOperationException("Event loop not initialized");
            }
            using PyObject taskFunc = loop.GetAttr("create_task");
            using PyObject task = taskFunc.Call(coroutine);
            using PyObject runUntilComplete = loop.GetAttr("run_until_complete");

            // On cancellation, call task cancellation in Python
            cancellationToken?.Register(() =>
                {
                    using PyObject cancel = task.GetAttr("cancel");
                    // TODO : send optional message
                    cancel.Call();
                });

            return runUntilComplete.Call(task);
        }
    }

    private static ConcurrentBag<EventLoop> eventLoops = [];
    [ThreadStatic] private static EventLoop? currentEventLoop = null;
    private static PyObject? AsyncioModule = null;
    private static PyObject? NewEventLoopFactory = null;

    internal static EventLoop GetEventLoop()
    {
        if (AsyncioModule is null)
        {
            throw new InvalidOperationException("Asyncio module not initialized");
        }
        if (currentEventLoop is null || currentEventLoop.IsDisposed)
        {
            currentEventLoop = new EventLoop();
            eventLoops.Add(currentEventLoop);
        }
        return currentEventLoop!;
    }

    internal static void CloseEventLoops()
    {
        foreach (var eventLoop in eventLoops)
        {
            eventLoop.Dispose();
        }
        eventLoops.Clear();
    }
}
