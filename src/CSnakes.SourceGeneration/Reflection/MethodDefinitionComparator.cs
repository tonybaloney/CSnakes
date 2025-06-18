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
    private static bool TypesAreEquivalent(TypeSyntax left, TypeSyntax right)
    {
        // If either side is a nullable type, we compare the element types
        if (left is NullableTypeSyntax)
            left = (left as NullableTypeSyntax)?.ElementType ?? left;
        if (right is NullableTypeSyntax)
            right = (right as NullableTypeSyntax)?.ElementType ?? right;

        if (left.Kind() != right.Kind())
            return false;
        return (left.Kind()) switch
        {
            SyntaxKind.PredefinedType => (left as PredefinedTypeSyntax)?.Keyword.Text == (right as PredefinedTypeSyntax)?.Keyword.Text,
            SyntaxKind.GenericName => (left as GenericNameSyntax)?.Identifier.Text == (right as GenericNameSyntax)?.Identifier.Text,
            _ => left.ToFullString() == right.ToFullString(), // For other types, we just compare the full string representation
        };
    }

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
            if (p1.Type is null && p2.Type is not null ||
                p1.Type is not null && p2.Type is null)
                return false; // If either type is null, they are not equivalent
            if (p1.Type is null && p2.Type is null)
                return true; // Both types are null, so they are equivalent
            return TypesAreEquivalent(p1.Type!, p2.Type!);
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
