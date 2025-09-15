using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSnakes.Reflection;

public class ArgumentReflection
{
    private static readonly TypeSyntax ReadOnlySpanOfKeywordArg = SyntaxFactory.ParseTypeName("ReadOnlySpan<KeywordArg>");
    private static readonly TypeSyntax ReadOnlySpanOfPyObject = SyntaxFactory.ParseTypeName("ReadOnlySpan<PyObject>");

    public static IEnumerable<ParameterSyntax> ArgumentSyntax(PythonFunctionParameter parameter) =>
        ArgumentSyntax(parameter, PythonFunctionParameterType.Normal);

    public static IEnumerable<ParameterSyntax> ArgumentSyntax(PythonFunctionParameter parameter,
                                                              PythonFunctionParameterType parameterType)
    {
        // Treat *args as list<Any>=None and **kwargs as dict<str, Any>=None
        // TODO: Handle the user specifying *args with a type annotation like tuple[int, str]
        const TypeReflection.ConversionDirection conversionDirection = TypeReflection.ConversionDirection.ToPython;

        var typeDefaultSyntaxPairs =
            (parameterType, parameter) switch
            {
                (PythonFunctionParameterType.Star, _) =>
                    [(ReadOnlySpanOfPyObject, SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))],
                (PythonFunctionParameterType.DoubleStar, _) =>
                    [(ReadOnlySpanOfKeywordArg, SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression))],
                (PythonFunctionParameterType.Normal, { ImpliedTypeSpec: var type, DefaultValue: null }) =>
                    from t in TypeReflection.AsPredefinedType(type, conversionDirection, RefSafetyContext.RefSafe)
                    select (Type: t, LiteralExpression: (LiteralExpressionSyntax?)null),
                (PythonFunctionParameterType.Normal, { ImpliedTypeSpec: var type, DefaultValue: var dv }) =>
                    // Defaults with literal expressions should be yielded on the matching type,
                    // E.g. Union[str, int] = 3
                    // Should yield
                    //    ParameterSyntax: int x = 3
                    //    ParameterSyntax: str x
                    // This should stop multiple defaults being yielded for the same parameter, which would
                    // cause a compilation error.
                    // Also, this would stop something like "string y = 3" which is also invalid.
                    from t in TypeReflection.AsPredefinedType(type, conversionDirection)
                    select (Type: t, LiteralExpression: (dv, t) switch
                    {
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
                    }),
                _ => throw new NotImplementedException()
            };

        return
            from tds in typeDefaultSyntaxPairs
            // If the default value is a literal expression, but we could not resolve the type to a builtin type,
            // avoid CS1750 (no standard conversion to PyObject)
            select tds is (IdentifierNameSyntax, { } le) && !le.IsKind(SyntaxKind.NullLiteralExpression)
                 ? tds with { LiteralExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression) }
                 : tds
            into td
            // If we ended up with PyObject x = null, we need to ensure the argument is nullable
            select td is (not NullableTypeSyntax, { } le) && le.IsKind(SyntaxKind.NullLiteralExpression)
                 ? td with { Type = SyntaxFactory.NullableType(td.Type) }
                 : td
            into syntax
            select SyntaxFactory.Parameter(SyntaxFactory.Identifier(Keywords.ValidIdentifier(parameter.Name.ToLowerPascalCase())))
                                .WithType(syntax.Type)
                                .WithDefault(syntax.LiteralExpression is { } le ? SyntaxFactory.EqualsValueClause(le) : null);
    }
}
