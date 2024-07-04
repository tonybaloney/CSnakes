
namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionDefinition
{
    public string Name { get; set; }

    public PythonFunctionParameter[] Parameters { get; set; }

    public PythonTypeSpec ReturnType { get; set; }

    public bool IsAsync { get; set; }
}
