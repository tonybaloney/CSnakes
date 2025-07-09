using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

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
        CreatePythonTypeDefinitionParser(Parse.Ref(() => PythonTypeDefinitionParser)).Named("Type Definition");

    public static TokenListParser<PythonToken, PythonTypeSpec> PythonReturnTypeDefinitionParser { get; } =
        CreatePythonTypeDefinitionParser(Parse.Ref(() => PythonReturnTypeDefinitionParser), allowForwardReferences: true).Named("Type Definition");

    private static TokenListParser<PythonToken, PythonTypeSpec>
        CreatePythonTypeDefinitionParser(TokenListParser<PythonToken, PythonTypeSpec> typeDefinitionParser,
                                         bool allowForwardReferences = false)
    {
        var nameParser =
            from t in Parse.OneOf(Token.EqualTo(PythonToken.None),
                                  Token.EqualTo(PythonToken.Identifier),
                                  Token.EqualTo(PythonToken.QualifiedIdentifier))
            select t.ToStringValue();

        var parser =
            from name in allowForwardReferences
                       ? nameParser.Or(from t in Token.EqualTo(PythonToken.DoubleQuotedString)
                                       select t.ToStringValue()[1..^1])
                       : nameParser
            from type in name switch
            {
                "None" => Parse.Return<PythonToken, PythonTypeSpec>(PythonTypeSpec.None),
                "Callable" and var n =>
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
                    select new PythonTypeSpec(n, [..callable.Parameters.Append(callable.Return)]),
                "Literal" =>
                    // Literal can contain any PythonConstant, or a list of them.
                    // See PEP586 https://peps.python.org/pep-0586/
                    // It can also contain an expression, like a typedef but we don't support that yet.
                    // It could also contain another Literal, but we don't support that yet either.
                    from literals in
                        ConstantValueTokenizer.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                              .Subscript()
                    select new PythonTypeSpec("Literal", [PythonTypeSpec.Literal([.. literals])]),
                "Union" =>
                    from subscript in
                        typeDefinitionParser.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                            .Subscript()
                    select PythonTypeSpec.Union(subscript),
                var other =>
                    from subscript in
                        typeDefinitionParser.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Comma))
                                            .Subscript()
                                            .OptionalOrDefault([])
                    select new PythonTypeSpec(other, [..subscript]),
            }
            select type;

        return from ts in parser.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Pipe))
               select ts switch
               {
                   [var single] => single,
                   var multiple => PythonTypeSpec.Union([..multiple])
               };
    }

    private static TokenListParser<PythonToken, T> Subscript<T>(this TokenListParser<PythonToken, T> parser) =>
        parser.Between(Token.EqualTo(PythonToken.OpenBracket), Token.EqualTo(PythonToken.CloseBracket));
}
