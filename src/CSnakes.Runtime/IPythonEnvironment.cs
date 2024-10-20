using CSnakes.Runtime.CPython;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

public interface IPythonEnvironment : IDisposable
{
    /// <summary>
    /// The version of the Python interpreter, which is a string that looks something like,
    /// <c>"3.0a5+ (py3k:63103M, May 12 2008, 00:53:55) \n[GCC 4.2.3]"</c>. The first word (up to
    /// the first space character) is the current Python version; the first characters are the major
    /// and minor version separated by a period.
    /// </summary>
    public string Version
    {
        get
        {
            return CPythonAPI.Py_GetVersion() ?? "No version available";
        }
    }

    public bool IsDisposed();

    public ILogger<IPythonEnvironment> Logger { get; }
}
