using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime;

public interface IPythonEnvironment
{
    public string Version()
    {
        if (!CPythonAPI.IsInitialized)
            return "Python not initialized";
        return CPythonAPI.Py_GetVersion();
    }
}
