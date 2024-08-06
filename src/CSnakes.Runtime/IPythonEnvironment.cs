using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime;

public interface IPythonEnvironment
{
    public string Version()
    {
        if (CPythonAPI.Py_IsInitialized() == 0)
            return "Python not initialized";
        return CPythonAPI.Py_GetVersion();
    }
}
