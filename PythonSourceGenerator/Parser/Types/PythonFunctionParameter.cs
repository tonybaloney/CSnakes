namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionParameter
{
    public string Name { get; set; }

    public string Type { get; set; }

    public bool IsPositionalOnly { get; set; }

    public bool IsKeywordOnly { get; set; }
}
