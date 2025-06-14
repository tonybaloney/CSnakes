using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;

/// <summary>
/// Compares method definitions based on their parameter uniqueness.
/// Avoids issues with invalid overloads.
/// </summary>
public class MethodDefinitionComparator
     : IEqualityComparer<MethodDefinition>
{
    public bool Equals(MethodDefinition x, MethodDefinition other)
    {
        // Different names
        if (x.Syntax.Identifier.Text != other.Syntax.Identifier.Text)
            return false;

        // Absolute match
        if (x.Syntax.ToFullString() == other.Syntax.ToFullString())
            return true;

        // Different number of parameters
        if (x.Syntax.ParameterList.Parameters.Count != other.Syntax.ParameterList.Parameters.Count)
            return false;

        // Are all the parameter types the same?
        return x.Syntax.ParameterList.Parameters.Zip(other.Syntax.ParameterList.Parameters, (p1, p2) =>
        {
            if (p1.Type?.Kind() != p2.Type?.Kind())
                return false;

            return (p1.Type?.Kind()) switch
            {
                SyntaxKind.PredefinedType => (p1.Type as PredefinedTypeSyntax)?.Keyword.Text == (p2.Type as PredefinedTypeSyntax)?.Keyword.Text,
                SyntaxKind.GenericName => (p1.Type as GenericNameSyntax)?.Identifier.Text == (p2.Type as GenericNameSyntax)?.Identifier.Text,
                SyntaxKind.NullableType => (p1.Type as NullableTypeSyntax)?.ElementType.ToFullString() == (p2.Type as NullableTypeSyntax)?.ElementType.ToFullString(),
                _ => p1.Type?.ToFullString() == p2.Type?.ToFullString(),// For other types, we just compare the full string representation
            };
        }).All(p => p);
    }

    private int GetHashCodeForParameterSyntax(ParameterSyntax parameter)
    {
        return parameter.Type?.Kind() switch
        {
            SyntaxKind.PredefinedType => (parameter.Type as PredefinedTypeSyntax)?.Keyword.Text.GetHashCode() ?? 0,
            SyntaxKind.GenericName => (parameter.Type as GenericNameSyntax)?.Identifier.Text.GetHashCode() ?? 0,
            SyntaxKind.NullableType => (parameter.Type as NullableTypeSyntax)?.ElementType.ToFullString().GetHashCode() ?? 0,
            _ => parameter.Type?.ToFullString().GetHashCode() ?? 0,
        };
    }

    /// <summary>
    /// Hash code for a method definition is based on the parameter types and the method name.
    /// See <see cref="Equals(MethodDefinition, MethodDefinition)"/> for the equality logic.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(MethodDefinition obj)
    {
        var parameterHashCodes = obj.Syntax.ParameterList.Parameters
            .Select(GetHashCodeForParameterSyntax)
            .Aggregate(0, (current, hash) => current ^ hash);
        return parameterHashCodes ^ obj.Syntax.Identifier.Text.GetHashCode();
    }
}
