using System.Diagnostics;

namespace CSnakes.Runtime;

[DebuggerDisplay("Exception Type={ExceptionType,nq}, Message={Message,nq}")]
public class PythonException(string exceptionType, string message, string pythonStackTrace) : Exception(message)
{
    public string ExceptionType { get; } = exceptionType;
    public string PythonStackTrace { get; } = pythonStackTrace;

    public override string ToString() => $"{ExceptionType}: {Message}\n{PythonStackTrace}";
}
