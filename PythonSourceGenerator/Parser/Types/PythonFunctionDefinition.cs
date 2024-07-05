
namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionDefinition
{
    PythonTypeSpec _returnType;

    public string Name { get; set; }

    public PythonFunctionParameter[] Parameters { get; set; }

    public PythonTypeSpec ReturnType
    {
        get { return _returnType ?? new PythonTypeSpec { Name = "Any" }; }
        set => _returnType = value;
    }

    public bool HasReturnTypeAnnotation()
    {
        return _returnType != null;
    }

    public bool IsAsync { get; set; }
}
