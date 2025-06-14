using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Collections.Immutable;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TextParser<TextSpan> Identifier { get; } =
        Span.MatchedBy(
            Character.Letter.Or(Character.EqualTo('_'))
            .IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('_')).Many())
        );

    public static TextParser<TextSpan> QualifiedName { get; } =
        Span.MatchedBy(
            Character.Letter.Or(Character.EqualTo('_'))
            .IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('_')).Many())
            .AtLeastOnceDelimitedBy(Character.EqualTo('.'))
        );

    public static TokenListParser<PythonToken, PythonTypeSpec> PythonTypeDefinitionParser { get; } =
        CreatePythonTypeDefinitionParser().Named("Type Definition");

    private static TokenListParser<PythonToken, PythonTypeSpec> CreatePythonTypeDefinitionParser()
    {
        var typeDefinitionParser = Parse.Ref(() => PythonTypeDefinitionParser);

        // Parser for anything but "None"

        var someParser =
            from name in Token.EqualTo(PythonToken.Identifier)
                              .Or(Token.EqualTo(PythonToken.QualifiedIdentifier))
            select name.ToStringValue() into name
            from args in name switch
            {
                "Callable" =>
                    // > The subscription syntax must always be used with exactly two values: the argument list and the
                    // > return type. The argument list must be a list of types, a `ParamSpec`, `Concatenate`, or an
                    // > ellipsis. The return type must be a single type.
                    //
                    // Source: Annotating callable objects,
                    // <https://docs.python.org/3/library/typing.html#annotating-callable-objects>
                    from callable in
                             typeDefinitionParser.ManyDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                                 .Subscript()
                                                 .ThenIgnore(Token.EqualTo(PythonToken.Comma))
                                                 .Then(ps => from r in typeDefinitionParser
                                                             select (Parameters: ps, Return: r))
                                                 .Subscript()
                    select ImmutableArray.CreateRange(callable.Parameters.Append(callable.Return)),
                "Literal" =>
                    // Literal can contain any PythonConstant, or a list of them.
                    // See PEP586 https://peps.python.org/pep-0586/
                    // It can also contain an expression, like a typedef but we don't support that yet.
                    // It could also contain another Literal, but we don't support that yet either.
                    from literals in
                        ConstantValueTokenizer.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                              .Subscript()
                    select ImmutableArray.CreateRange([PythonTypeSpec.Literal([.. literals])]),
                _ =>
                    from subscript in
                        typeDefinitionParser.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                            .Subscript()
                                            .OptionalOrDefault([])
                    select ImmutableArray.Create(subscript)
            }
            select new PythonTypeSpec(name, args);

        // Parser for: x | None

        var unionNoneParser =
            from t in someParser.ThenIgnore(Token.EqualTo(PythonToken.Pipe))
                                .ThenIgnore(Token.EqualTo(PythonToken.None))
            select PythonTypeSpec.Optional(t);

        // Parser for: None | x

        var noneUnionParser =
            from t in Token.EqualTo(PythonToken.None)
                           .IgnoreThen(Token.EqualTo(PythonToken.Pipe))
                           .IgnoreThen(someParser)
            select PythonTypeSpec.Optional(t);

        // Parser for one of the above or "None"

        return Parse.OneOf(noneUnionParser.Try(),
                           unionNoneParser.Try(),
                           Token.EqualTo(PythonToken.None)
                                .IgnoreThen(Parse.Return<PythonToken, PythonTypeSpec>(PythonTypeSpec.None)),
                           someParser);
    }

    private static TokenListParser<PythonToken, T> Subscript<T>(this TokenListParser<PythonToken, T> parser) =>
        parser.Between(Token.EqualTo(PythonToken.OpenBracket), Token.EqualTo(PythonToken.CloseBracket));
}
