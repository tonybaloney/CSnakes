using CSnakes.Runtime.Python;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

[DebuggerDisplay("Exception Type={ExceptionType,nq}, Message={Message,nq}")]
public class PythonException(string exceptionType, string message, PyObject pythonStackTrace) : Exception(message)
{
    private string[]? formattedStackTrace = null;
    public string ExceptionType { get; } = exceptionType;
    public string[] PythonStackTrace { get
        {
            // This is lazy because it's expensive to format the stack trace.
            formattedStackTrace ??= FormatPythonStackTrace(pythonStackTrace);
            return formattedStackTrace;
        }
    }

    private static string[] FormatPythonStackTrace(PyObject pythonStackTrace)
    {
        using (GIL.Acquire())
        {
            using var tracebackModule = Import.ImportModule("traceback");
            using var formatTbFunction = tracebackModule.GetAttr("format_tb");
            using var formattedStackTrace = formatTbFunction.Call(pythonStackTrace);
        
            string[] result = formattedStackTrace.As<IEnumerable<string>>().ToArray();
            return result;
        }
    }

    public override string ToString() => $"{ExceptionType}: {Message}\n{PythonStackTrace}";
}
