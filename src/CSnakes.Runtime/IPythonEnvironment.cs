using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime;

public interface IPythonEnvironment
{
    public string Version()
    {
        return CPythonAPI.Py_GetVersion();
    }
}
