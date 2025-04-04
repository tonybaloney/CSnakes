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

        return
            from name in Parse.OneOf(Token.EqualTo(PythonToken.Identifier),
                                     Token.EqualTo(PythonToken.QualifiedIdentifier),
                                     Token.EqualTo(PythonToken.None))
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
                             .Then(a => from b in typeDefinitionParser
                                        select (Parameters: a, Return: b))
                             .Subscript()
                    select ImmutableArray.CreateRange(callable.Parameters.Append(callable.Return)),
                _ =>
                    from subscript in
                        typeDefinitionParser.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                            .Subscript()
                                            .OptionalOrDefault([])
                    select ImmutableArray.Create(subscript)
            }
            select new PythonTypeSpec(name, args);
    }

    private static TokenListParser<PythonToken, T> Subscript<T>(this TokenListParser<PythonToken, T> parser) =>
        parser.Between(Token.EqualTo(PythonToken.OpenBracket), Token.EqualTo(PythonToken.CloseBracket));
}
