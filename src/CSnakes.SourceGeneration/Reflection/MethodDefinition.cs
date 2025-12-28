using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;
public class MethodDefinition(MethodDeclarationSyntax syntax,
                              PythonFunctionDefinition pythonFunction)
{
    public MethodDeclarationSyntax Syntax { get; } = syntax;

    public PythonFunctionDefinition PythonFunction { get; } = pythonFunction;
}
