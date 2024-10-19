using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSnakes.Parser.Types;

namespace CSnakes.Reflection;

public class ArgumentReflection
{
    private static readonly PythonTypeSpec DictStrAny = new("dict", [new("str", []), PythonTypeSpec.Any]);
    private static readonly TypeSyntax ArrayPyObject = SyntaxFactory.ParseTypeName("PyObject[]");

    public static ParameterSyntax? ArgumentSyntax(PythonFunctionParameter parameter)
    {
        // Treat *args as list<Any>=None and **kwargs as dict<str, Any>=None
        // TODO: Handle the user specifying *args with a type annotation like tuple[int, str]
        var (reflectedType, defaultValue) = parameter switch
        {
            PythonFunctionParameter.Star => (ArrayPyObject, PythonConstant.None.Value),
            PythonFunctionParameter.DoubleStar => (TypeReflection.AsPredefinedType(DictStrAny, TypeReflection.ConversionDirection.ToPython), PythonConstant.None.Value),
            PythonFunctionParameter.Normal { Type: var type, DefaultValue: var dv } => (TypeReflection.AsPredefinedType(type, TypeReflection.ConversionDirection.ToPython), dv),
            _ => throw new NotImplementedException()
        };

        bool isNullableType = false;

        LiteralExpressionSyntax? literalExpressionSyntax;

        switch (defaultValue)
        {
            case null:
                literalExpressionSyntax = null;
                break;
            case PythonConstant.HexidecimalInteger { Value: var v }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal($"0x{v:X}", v));
                break;
            case PythonConstant.BinaryInteger { Value: var v }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal($"0b{Convert.ToString(v, 2)}", v));
                break;
            case PythonConstant.Integer { Value: var v and >= int.MinValue and <= int.MaxValue }:
                // Downcast long to int if the value is small as the code is more readable without the L suffix
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal((int)v));
                break;
            case PythonConstant.Integer { Value: var v }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal(v));
                break;
            case PythonConstant.String { Value: var v }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal(v));
                break;
            case PythonConstant.Float { Value: var v }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        SyntaxFactory.Literal(v));
                break;
            case PythonConstant.Bool { Value: true }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
                break;
            case PythonConstant.Bool { Value: false }:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                break;
            case PythonConstant.None:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                isNullableType = true;
                break;
            default:
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                isNullableType = true;
                break;
        }

        return SyntaxFactory
            .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
            .WithType(isNullableType ? SyntaxFactory.NullableType(reflectedType) : reflectedType)
            .WithDefault(literalExpressionSyntax is not null ? SyntaxFactory.EqualsValueClause(literalExpressionSyntax) : null);
    }

    public static List<(PythonFunctionParameterListEntryKind, PythonFunctionParameter, ParameterSyntax)> FunctionParametersAsParameterSyntaxPairs(PythonFunctionParameterList parameters)
    {
        List<(PythonFunctionParameterListEntryKind, PythonFunctionParameter, ParameterSyntax)> parametersList = [];
        foreach (var (kind, parameter) in parameters)
        {
            var argument = ArgumentSyntax(parameter);
            if (argument != null)
            {
                parametersList.Add((kind, parameter, argument));
            }
        }

        return parametersList;
    }
}
