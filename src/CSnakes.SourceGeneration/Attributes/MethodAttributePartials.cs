using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.SourceGeneration.Attributes;
internal class MethodAttributePartials
{
    internal class PythonMethod(string Namespace, string ClassName, string MethodName)
    {
        internal string Namespace { get; } = Namespace;
        internal string ClassName { get; } = ClassName;
        internal string MethodName { get; } = MethodName;

    }

    internal static bool CouldBeMethod(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is MethodDeclarationSyntax methodDeclaration &&
            methodDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    internal static PythonMethod GetMethodInfo(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var type = (INamedTypeSymbol)context.TargetSymbol;
        var containingClass = context.TargetSymbol.ContainingType;
        return new PythonMethod(
            // Note: this is a simplified example. You will also need to handle the case where the type is in a global namespace, nested, etc.
            Namespace: containingClass.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
            ClassName: containingClass.Name,
            MethodName: context.TargetSymbol.Name);
    }
}
