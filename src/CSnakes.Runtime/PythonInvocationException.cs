using CSnakes.Runtime.Python;
using System.Diagnostics;

namespace CSnakes.Runtime;

[DebuggerDisplay("Exception Type={PythonExceptionType,nq}, Message={Message,nq}")]
public class PythonInvocationException : Exception
{
    public PythonInvocationException(string exceptionType, string message, PyObject? pythonStackTrace) : base($"The Python runtime raised a {exceptionType} exception, see InnerException for details.", new PythonRuntimeException(message, pythonStackTrace))
    {
        PythonExceptionType = exceptionType;
    }

    public string PythonExceptionType { get; }
}
