using System.Collections.Generic;

namespace PythonSourceGenerator.Parser.Types;
public class PythonFunctionDefinition
{
    public string Name { get; set; }

    public PythonFunctionParameter[] Parameters { get; set; }

    public string ReturnType { get; set; }

    public bool IsAsync { get; set; }
}
