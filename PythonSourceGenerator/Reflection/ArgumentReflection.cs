using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;

namespace PythonSourceGenerator.Reflection;

public class ArgumentReflection
{
    private static readonly PythonTypeSpec TupleAny = new("tuple", [PythonTypeSpec.Any]);
    private static readonly PythonTypeSpec DictStrAny = new("dict", [new("str", []), PythonTypeSpec.Any]);

    public static ParameterSyntax ArgumentSyntax(PythonFunctionParameter parameter)
    {
        // Treat *args as tuple<Any> and **kwargs as dict<str, Any>
        TypeSyntax reflectedType = parameter.ParameterType switch
        {
            PythonFunctionParameterType.Star => TypeReflection.AsPredefinedType(TupleAny),
            PythonFunctionParameterType.DoubleStar => TypeReflection.AsPredefinedType(DictStrAny),
            PythonFunctionParameterType.Normal => TypeReflection.AsPredefinedType(parameter.Type),
            _ => throw new System.NotImplementedException()
        };

        if (parameter.DefaultValue == null)
        {
            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
                .WithType(reflectedType);
        }
        else
        {
            LiteralExpressionSyntax literalExpressionSyntax;
            if (parameter.DefaultValue.IsInteger)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(parameter.DefaultValue.IntegerValue));
            else if (parameter.DefaultValue.IsString && parameter.DefaultValue.StringValue is not null)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            SyntaxFactory.Literal(parameter.DefaultValue.StringValue));
            else if (parameter.DefaultValue.IsFloat)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(parameter.DefaultValue.FloatValue));
            else if (parameter.DefaultValue.IsBool && parameter.DefaultValue.BoolValue == true)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
            else if (parameter.DefaultValue.IsBool && parameter.DefaultValue.BoolValue == false)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
            else if (parameter.DefaultValue.IsNone)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            else
                // TODO : Handle other types?
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
                .WithType(reflectedType)
                .WithDefault(SyntaxFactory.EqualsValueClause(literalExpressionSyntax));

        }
    }

    public static ParameterListSyntax ParameterListSyntax(PythonFunctionParameter[] parameters)
    {
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters.Select(ArgumentSyntax).ToList()));
    }
}
