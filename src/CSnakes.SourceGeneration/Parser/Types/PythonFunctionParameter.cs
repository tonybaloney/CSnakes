namespace CSnakes.Parser.Types;
public class PythonFunctionParameter(string name, PythonTypeSpec? type, PythonConstant? defaultValue, PythonFunctionParameterType parameterType)
{
    public string Name { get; } = name;

    public PythonTypeSpec Type { get; } = type ?? PythonTypeSpec.Any;

    public bool IsPositionalOnly { get; set; }

    public bool IsKeywordOnly { get; set; }

    public PythonConstant? DefaultValue { get; set; } = defaultValue;

    public PythonFunctionParameterType ParameterType { get; } = parameterType;

    public bool HasTypeAnnotation() => type is not null;
}
