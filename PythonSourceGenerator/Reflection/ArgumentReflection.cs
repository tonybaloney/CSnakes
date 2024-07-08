using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection;

public class ArgumentReflection
{
    public static ParameterSyntax ArgumentSyntax(PythonFunctionParameter parameter)
    {
        var reflectedType = TypeReflection.AsPredefinedType(parameter.Type);

        if (parameter.DefaultValue == null)
        {
            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(parameter.Name.ToLowerPascalCase()))
                .WithType(reflectedType);
        } else
        {
            LiteralExpressionSyntax literalExpressionSyntax;
            if (parameter.DefaultValue.IsInteger)
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(parameter.DefaultValue.IntegerValue));
            else if (parameter.DefaultValue.IsString)
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
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NullLiteralExpression);
            else
                // TODO : Handle other types?
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NullLiteralExpression);
            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(parameter.Name.ToLowerPascalCase()))
                .WithType(reflectedType)
                .WithDefault(SyntaxFactory.EqualsValueClause(literalExpressionSyntax));

        }
    }

    public static ParameterListSyntax ParameterListSyntax(PythonFunctionParameter[] parameters)
    {
        var parameterListSyntax = new List<ParameterSyntax>();
        foreach (var pythonParameter in parameters)
        {
            // TODO : Handle Kind, see https://docs.python.org/3/library/inspect.html#inspect.Parameter
            parameterListSyntax.Add(ArgumentSyntax(pythonParameter));
        }
        return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterListSyntax));
    }
}
