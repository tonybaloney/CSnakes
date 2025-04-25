using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static bool IsPyCoroutine(PyObject p)
    {
        return HasAttr(p, "__await__");
    }

    private static readonly Lock defaultEventLoopLock = new();
    private static EventLoop? defaultEventLoop = null;
    private static PyObject? AsyncioModule = null;
    private static PyObject? NewEventLoopFactory = null;

    internal static EventLoop GetDefaultEventLoop()
    {
        if (AsyncioModule is null)
        {
            throw new InvalidOperationException("Asyncio module not initialized");
        }

        lock (defaultEventLoopLock)
            return defaultEventLoop ??= EventLoop.RunNewForever();
    }

    internal static void CloseEventLoops()
    {
        EventLoop? defaultEventLoop;

        lock (defaultEventLoopLock)
        {
            defaultEventLoop = CPythonAPI.defaultEventLoop;
            CPythonAPI.defaultEventLoop = null;
        }

        if (defaultEventLoop is { } someDefaultEventLoop)
            someDefaultEventLoop.Dispose();
    }

    internal static PyObject NewEventLoop() =>
        NewEventLoopFactory switch
        {
            null => throw new InvalidOperationException($"{nameof(NewEventLoopFactory)} not initialized."),
            var some => some.Call(),
        };
}
