using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;
public class PythonRuntimeException : Exception
{
    private readonly PyObject? pythonTracebackObject;
    private string[]? formattedStackTrace = null;

    public PythonRuntimeException(PyObject? exception, PyObject? traceback) : base(exception?.ToString(), GetPythonInnerException(exception))
    {
        pythonTracebackObject = traceback;
        if (traceback is null)
        {
            return;
        }

        using (GIL.Acquire())
        {
            Data["locals"] = PyObjectImporters.Mapping<string, PyObject, PyObjectImporters.String, PyObjectImporters.Clone>.BareImport(traceback.GetAttr("tb_frame").GetAttr("f_locals"));
            Data["globals"] = PyObjectImporters.Mapping<string, PyObject, PyObjectImporters.String, PyObjectImporters.Clone>.BareImport(traceback.GetAttr("tb_frame").GetAttr("f_globals"));
        }
    }

    private static PythonRuntimeException? GetPythonInnerException(PyObject? exception) =>
        exception is { } someException
        && someException.HasAttr("__cause__")
        && someException.GetAttr("__cause__") is var cause
        && !cause.IsNone()
            ? new PythonRuntimeException(cause, null)
            : null;

    public string[] PythonStackTrace
    {
        get
        {
            if (pythonTracebackObject is null)
            {
                return [];
            }
            // This is lazy because it's expensive to format the stack trace.
            formattedStackTrace ??= FormatPythonStackTrace(pythonTracebackObject);
            return formattedStackTrace;
        }
    }

    private static string[] FormatPythonStackTrace(PyObject pythonStackTrace)
    {
        if (!CPythonAPI.IsInitialized)
        {
            return [];
        }

        using (GIL.Acquire())
        {
            using var tracebackModule = Import.ImportModule("traceback");
            using var formatTbFunction = tracebackModule.GetAttr("format_tb");
            using var formattedStackTrace = formatTbFunction.Call(pythonStackTrace);

            return [.. PyObjectImporters.List<string, PyObjectImporters.String>.BareImport(formattedStackTrace)];
        }
    }

    public override string? StackTrace => string.Join(Environment.NewLine, PythonStackTrace);
}
