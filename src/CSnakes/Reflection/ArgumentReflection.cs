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
                    break;
                case PythonConstant.ConstantType.HexidecimalInteger:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(string.Format("0x{0:X}", parameter.DefaultValue.IntegerValue), parameter.DefaultValue.IntegerValue));
                    break;
                case PythonConstant.ConstantType.BinaryInteger:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.NumericLiteralExpression,
                                                            SyntaxFactory.Literal(string.Format("0b{0:B}", parameter.DefaultValue.IntegerValue), parameter.DefaultValue.IntegerValue));
                    break;
                default:
                    literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                    break;
            }


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
