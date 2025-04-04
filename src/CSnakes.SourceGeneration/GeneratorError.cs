namespace CSnakes;

public class GeneratorError
{

    public string Message { get; }

    public int StartLine { get; }

    public int StartColumn { get; }

    public int EndLine { get; }

    public int EndColumn { get; }

    public string Code { get; }

    public GeneratorError(int startLine, int endLine, int startColumn, int endColumn, string message)
    {
        Message = message;
        StartLine = startLine;
        StartColumn = startColumn;
        EndLine = endLine == -1 ? startLine : endLine;
        EndColumn = endColumn;
        Code = "hello";
    }
}
