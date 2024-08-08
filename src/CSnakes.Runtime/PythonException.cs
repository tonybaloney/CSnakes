namespace CSnakes.Runtime;

public class PythonException : Exception
{
    public PythonException(string exceptionType, string message, string stackTrace) : base(message)
    {
        // TODO: Handle exception type
        // TODO: Custom set stack trace...
    }
}
