namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionParameter(string name, PythonTypeSpec? type, PythonConstant defaultValue, bool isStar, bool isDoubleStar)
{
    public string Name { get; } = name;

    public PythonTypeSpec Type { get;  } = type ?? PythonTypeSpec.Any;

    public bool IsPositionalOnly { get; set; }

    public bool IsKeywordOnly { get; set; }

    public PythonConstant DefaultValue { get; set; } = defaultValue;

    public bool IsStar { get; } = isStar;

    public bool IsDoubleStar { get; } = isDoubleStar;

    public bool HasTypeAnnotation() => type is not null;
}
