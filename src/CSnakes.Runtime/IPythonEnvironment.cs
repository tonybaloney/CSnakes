using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime;

public interface IPythonEnvironment
{
    public string Version
    {
        get {
            return CPythonAPI.Py_GetVersion() ?? "No version available";
        }
    }
}
