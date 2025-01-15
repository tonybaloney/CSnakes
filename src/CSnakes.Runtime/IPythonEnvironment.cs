using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
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

    public bool IsDisposed();

    public ILogger<IPythonEnvironment> Logger { get; }

    public PyObject Execute(string code);
    public PyObject Execute(string code, IDictionary<string, PyObject> globals, IDictionary<string, PyObject> locals);
}
