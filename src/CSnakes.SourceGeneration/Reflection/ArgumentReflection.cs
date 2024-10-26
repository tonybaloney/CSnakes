using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSnakes.Parser.Types;

namespace CSnakes.Reflection;

public class ArgumentReflection
{
    private static readonly TypeSyntax StarReflectedType = SyntaxFactory.ParseTypeName("PyObject[]?");
    private static readonly TypeSyntax DoubleStarReflectedType = SyntaxFactory.ParseTypeName("IReadOnlyDictionary<string, PyObject>?");

    public static ParameterSyntax? ArgumentSyntax(PythonFunctionParameter parameter)
    {
        // The parameter / is a special syntax, not a parameter.
        if (parameter.ParameterType == PythonFunctionParameterType.Slash)
        {
            return null;
        }

        // Treat *args as list<Any>=None and **kwargs as dict<str, Any>=None
        // TODO: Handle the user specifying *args with a type annotation like tuple[int, str]
        var (reflectedType, defaultValue) = parameter switch
        {
            { ParameterType: PythonFunctionParameterType.Star } => (StarReflectedType, PythonConstant.None.Value),
            { ParameterType: PythonFunctionParameterType.DoubleStar } => (DoubleStarReflectedType, PythonConstant.None.Value),
            { ParameterType: PythonFunctionParameterType.Normal, Type: var pt, DefaultValue: var dv } =>
                (TypeReflection.AsPredefinedType(pt, TypeReflection.ConversionDirection.ToPython), dv),
            _ => throw new NotImplementedException()
        };

        var literalExpressionSyntax = defaultValue switch
        {
            null => null,
            PythonConstant.HexidecimalInteger { Value: var v } =>
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal($"0x{v:X}", v)),
            PythonConstant.BinaryInteger { Value: var v } =>
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                SyntaxFactory.Literal($"0b{Convert.ToString(v, 2)}", v)),
            PythonConstant.Integer { Value: var v and >= int.MinValue and <= int.MaxValue } =>
                // Downcast long to int if the value is small as the code is more readable without the L suffix
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((int)v)),
            PythonConstant.Integer { Value: var v } =>
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
            PythonConstant.String { Value: var v } =>
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(v)),
            PythonConstant.Float { Value: var v } =>
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
            PythonConstant.Bool { Value: true } => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
            PythonConstant.Bool { Value: false } => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
            _ => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
        };

        return SyntaxFactory
            .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
            .WithType(reflectedType)
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
