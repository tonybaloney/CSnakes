using CSnakes.Runtime.CPython;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

public interface IPythonEnvironment : IDisposable
{
    public string Version
    {
        get
        {
            return CAPI.Py_GetVersion() ?? "No version available";
        }
    }

    public bool IsDisposed();

    public ILogger<IPythonEnvironment> Logger { get; }
}
