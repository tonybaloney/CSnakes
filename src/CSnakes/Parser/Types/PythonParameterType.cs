namespace PythonSourceGenerator.Parser.Types;

public class PythonParameterType(string name, PythonFunctionParameterType parameterType)
{
    public string Name { get; } = name;
    public PythonFunctionParameterType ParameterType { get; set; } = parameterType;
}
