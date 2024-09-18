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
            parameter.DefaultValue = PythonConstant.FromNone();
        }

        bool isNullableType = false;

        if (parameter.DefaultValue is null)
        {
            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
                .WithType(reflectedType);
        }
        else
        {
            LiteralExpressionSyntax literalExpressionSyntax;

            switch (parameter.DefaultValue.Type)
            {
                case PythonConstant.ConstantType.Integer:
                    // Downcast long to int if the value is small as the code is more readable without the L suffix
                    if (parameter.DefaultValue.IntegerValue <= int.MaxValue && parameter.DefaultValue.IntegerValue >= int.MinValue)
                        literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.NumericLiteralExpression,
                                                                SyntaxFactory.Literal((int)parameter.DefaultValue.IntegerValue));
                    else
                        literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.NumericLiteralExpression,
                                                                SyntaxFactory.Literal(parameter.DefaultValue.IntegerValue));
                    break;
                case PythonConstant.ConstantType.String:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            SyntaxFactory.Literal(parameter.DefaultValue.StringValue ?? String.Empty));
                    break;
                case PythonConstant.ConstantType.Float:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(parameter.DefaultValue.FloatValue));
                    break;
                case PythonConstant.ConstantType.Bool:
                    literalExpressionSyntax = parameter.DefaultValue.BoolValue
                        ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                        : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                    break;
                case PythonConstant.ConstantType.None:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                    isNullableType = true;
                    break;
                case PythonConstant.ConstantType.HexidecimalInteger:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(string.Format("0x{0:X}", parameter.DefaultValue.IntegerValue), parameter.DefaultValue.IntegerValue));
                    break;
                case PythonConstant.ConstantType.BinaryInteger:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(string.Format("0b{0}", Convert.ToString(parameter.DefaultValue.IntegerValue, 2)), parameter.DefaultValue.IntegerValue));
                    break;
                default:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                    isNullableType = true;
                    break;
            }

            if (isNullableType)
            {
                reflectedType = SyntaxFactory.NullableType(reflectedType);
            }

            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
                .WithType(reflectedType)
                .WithDefault(SyntaxFactory.EqualsValueClause(literalExpressionSyntax));

        }
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
