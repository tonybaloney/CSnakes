namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionParameter
{
    PythonTypeSpec _type;

    public string Name { get; set; }

    public PythonTypeSpec Type {
        get { return _type ?? new PythonTypeSpec { Name = "Any" }; }
        set => _type = value;
    }

    public bool IsPositionalOnly { get; set; }

    public bool IsKeywordOnly { get; set; }

    public PythonConstant? DefaultValue { get; set; }

    public bool IsStar { get; set; }

    public bool IsDoubleStar { get; set; }

    public bool HasTypeAnnotation()
    {
        return _type != null;
    }
}
