using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static bool IsPyCoroutine(PyObject p)
    {
        return HasAttr(p, "__await__");
    }

    private static PyObject? currentEventLoop = null;
    private static PyObject? asyncioModule = null;

    internal static PyObject GetAsyncioModule()
    {
        asyncioModule ??= Import("asyncio");
        return asyncioModule;
    }

    internal static PyObject GetCurrentEventLoop()
    {
        currentEventLoop ??= GetAsyncioModule().GetAttr("new_event_loop").Call();
        return currentEventLoop;
    }
}
