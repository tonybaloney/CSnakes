using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;

public class ArgumentReflection
{
    private static readonly PythonTypeSpec OptionalDictStrAny = PythonTypeSpec.Optional(new("dict", [new("str"), PythonTypeSpec.Any]));
    private static readonly TypeSyntax NullableArrayOfPyObject = SyntaxFactory.ParseTypeName("PyObject[]?");

    public static IEnumerable<ParameterSyntax> ArgumentSyntax(PythonFunctionParameter parameter) =>
        ArgumentSyntax(parameter, PythonFunctionParameterType.Normal);

    public static IEnumerable<ParameterSyntax> ArgumentSyntax(PythonFunctionParameter parameter,
                                                 PythonFunctionParameterType parameterType)
    {
        // Treat *args as list<Any>=None and **kwargs as dict<str, Any>=None
        // TODO: Handle the user specifying *args with a type annotation like tuple[int, str]
        const TypeReflection.ConversionDirection conversionDirection = TypeReflection.ConversionDirection.ToPython;

        var (reflectedType, defaultValue) = (parameterType, parameter) switch
        {
            (PythonFunctionParameterType.Star, _) => ([NullableArrayOfPyObject], PythonConstant.None.Value),
            (PythonFunctionParameterType.DoubleStar, _) => (TypeReflection.AsPredefinedType(OptionalDictStrAny, conversionDirection, RefSafetyContext.RefSafe), PythonConstant.None.Value),
            (PythonFunctionParameterType.Normal, { ImpliedTypeSpec: var type, DefaultValue: null }) => (TypeReflection.AsPredefinedType(type, conversionDirection, RefSafetyContext.RefSafe), null),
            (PythonFunctionParameterType.Normal, { ImpliedTypeSpec: var type, DefaultValue: var dv }) => (TypeReflection.AsPredefinedType(type, conversionDirection), dv),
            _ => throw new NotImplementedException()
        };

        foreach (var reflectedType in reflectedTypes)
        {
            // Defaults with literal expressions should be yielded on the matching type,
            // E.g. Union[str, int] = 3
            // Should yield
            //    ParameterSyntax: int x = 3
            //    ParameterSyntax: str x
            // This should stop multiple defaults being yielded for the same parameter, which would
            // cause a compilation error.
            // Also this would stop something like "string y = 3" which is also invalid.
            var literalExpressionSyntax = (defaultValue, reflectedType) switch
            {
                (null, _) => null,
                (PythonConstant.HexadecimalInteger { Value: var v },
                 PredefinedTypeSyntax { Keyword.Value: "long" }) =>
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal($"0x{v:X}", v)),
                (PythonConstant.BinaryInteger { Value: var v },
                 PredefinedTypeSyntax { Keyword.Value: "long" }) =>
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal($"0b{Convert.ToString(v, 2)}", v)),
                (PythonConstant.Integer { Value: var v and >= int.MinValue and <= int.MaxValue },
                 PredefinedTypeSyntax { Keyword.Value: "long" or "double" }) =>
                    // Downcast long to int if the value is small as the code is more readable without the L suffix
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((int)v)),
                (PythonConstant.Integer { Value: var v },
                 PredefinedTypeSyntax { Keyword.Value: "long" or "double" }) =>
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                (PythonConstant.String { Value: var v },
                 PredefinedTypeSyntax { Keyword.Value: "string" }) =>
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(v)),
                (PythonConstant.Float { Value: var v },
                 PredefinedTypeSyntax { Keyword.Value: "double" }) =>
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(v)),
                (PythonConstant.Bool { Value: true },
                 PredefinedTypeSyntax { Keyword.Value: "bool" }) => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
                (PythonConstant.Bool { Value: false },
                 PredefinedTypeSyntax { Keyword.Value: "bool" }) => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
                (_, _) => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
            };

            // Create a new variable to avoid modifying the foreach iteration variable
            var adjustedReflectedType = reflectedType;

            // If the default value is a literal expression, but we could not resolve the type to a builtin type,
            // avoid CS1750 (no standard conversion to PyObject)
            if (literalExpressionSyntax is not null
                && !literalExpressionSyntax.IsKind(SyntaxKind.NullLiteralExpression)
                && adjustedReflectedType is IdentifierNameSyntax)
            {
                literalExpressionSyntax = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }

            // If we ended up with PyObject x = null, we need to ensure the argument is nullable
            if (literalExpressionSyntax is not null
                && literalExpressionSyntax.IsKind(SyntaxKind.NullLiteralExpression)
                && adjustedReflectedType is not NullableTypeSyntax)
            {
                adjustedReflectedType = SyntaxFactory.NullableType(adjustedReflectedType);
            }

            yield return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
                .WithType(adjustedReflectedType)
                .WithDefault(literalExpressionSyntax is not null ? SyntaxFactory.EqualsValueClause(literalExpressionSyntax) : null);
        }
    }
}
