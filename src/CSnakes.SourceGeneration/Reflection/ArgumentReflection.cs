using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;

public class ArgumentReflection
{
    private static readonly PythonTypeSpec DictStrAny = new("dict", [new("str", []), PythonTypeSpec.Any]);
    private static readonly TypeSyntax ArrayPyObject = SyntaxFactory.ParseTypeName("PyObject[]");

    public static ParameterSyntax? ArgumentSyntax(PythonFunctionParameter parameter)
    {
        // The parameter / is a special syntax, not a parameter.
        if (parameter.ParameterType == PythonFunctionParameterType.Slash)
        {
            return null;
        }

        // Treat *args as list<Any>=None and **kwargs as dict<str, Any>=None
        // TODO: Handle the user specifying *args with a type annotation like tuple[int, str]
        TypeSyntax reflectedType = parameter.ParameterType switch
        {
            PythonFunctionParameterType.Star => ArrayPyObject,
            PythonFunctionParameterType.DoubleStar => TypeReflection.AsPredefinedType(DictStrAny, TypeReflection.ConversionDirection.ToPython),
            PythonFunctionParameterType.Normal => TypeReflection.AsPredefinedType(parameter.Type, TypeReflection.ConversionDirection.ToPython),
            _ => throw new NotImplementedException()
        };

        // Force a default value for *args and **kwargs as null, otherwise the calling convention is strange
        if ((parameter.ParameterType == PythonFunctionParameterType.Star ||
             parameter.ParameterType == PythonFunctionParameterType.DoubleStar) &&
            parameter.DefaultValue is null)

        {
            parameter.DefaultValue = PythonConstant.None.Value;
        }

        bool isNullableType = false;

        LiteralExpressionSyntax? literalExpressionSyntax;

        switch (parameter.DefaultValue)
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

    public static List<(PythonFunctionParameter, ParameterSyntax)> FunctionParametersAsParameterSyntaxPairs(PythonFunctionParameter[] parameters)
    {
        List<(PythonFunctionParameter, ParameterSyntax)> parametersList = [];
        foreach (var parameter in parameters)
        {
            var argument = ArgumentSyntax(parameter);
            if (argument != null)
            {
                parametersList.Add((parameter, argument));
            }
        }

        return parametersList;
    }
}
