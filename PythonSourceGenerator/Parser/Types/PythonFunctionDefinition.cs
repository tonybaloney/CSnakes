namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionDefinition(string name, PythonTypeSpec? returnType, PythonFunctionParameter[] pythonFunctionParameter)
{
    public string Name { get; } = name;

    public PythonFunctionParameter[] Parameters { get; } = pythonFunctionParameter;

    public PythonTypeSpec ReturnType { get; } = returnType ?? PythonTypeSpec.Any;

    public bool HasReturnTypeAnnotation() => returnType is not null;

    public bool IsAsync { get; set; }
}
