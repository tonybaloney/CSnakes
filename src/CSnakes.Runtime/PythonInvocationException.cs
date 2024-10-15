using CSnakes.Runtime.Python;
using System.Diagnostics;

namespace CSnakes.Runtime;

[DebuggerDisplay("Exception Type={PythonExceptionType,nq}, Message={Message,nq}")]
public class PythonInvocationException : Exception
{
    public PythonInvocationException(string exceptionType, PythonObject? exception, PythonObject? pythonStackTrace) : base($"The Python runtime raised a {exceptionType} exception, see InnerException for details.", new PythonRuntimeException(exception, pythonStackTrace))
    {
        PythonExceptionType = exceptionType;
    }

    public PythonInvocationException(string exceptionType, PythonObject? exception, PythonObject? pythonStackTrace, string customMessage) : base(customMessage, new PythonRuntimeException(exception, pythonStackTrace))
    {
        PythonExceptionType = exceptionType;
    }

    public string PythonExceptionType { get; }
}
