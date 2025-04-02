using CSnakes.Runtime.CPython;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

public interface IPythonEnvironment : IDisposable
{
    public string Version
    {
        get
        {
            return CPythonAPI.Py_GetVersion() ?? "No version available";
        }
    }

    public string ExecutablePath
    {
        get
        {
            return CPythonAPI.PythonExecutablePath;
        }
    }

    public bool IsDisposed();

    public ILogger<IPythonEnvironment> Logger { get; }
}
