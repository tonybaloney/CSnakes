using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;
public class MethodDefinition(MethodDeclarationSyntax syntax)
{
    public MethodDeclarationSyntax Syntax { get; } = syntax;
}
