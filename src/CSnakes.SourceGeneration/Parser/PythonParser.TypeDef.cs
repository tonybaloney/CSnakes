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

    public static TokenListParser<PythonToken, PythonTypeSpec> PythonTypeDefinitionParser { get; }

    private static TokenListParser<PythonToken, PythonTypeSpec> CreatePythonTypeDefinitionParser()
    {
        var typeDefinitionParser = Parse.Ref(() => PythonTypeDefinitionParser);

        var comma = Token.EqualTo(PythonToken.Comma);

        var parser =
            from name in Parse.OneOf(Token.EqualTo(PythonToken.None),
                                     Token.EqualTo(PythonToken.Identifier),
                                     Token.EqualTo(PythonToken.QualifiedIdentifier))
            from type in name.ToStringValue() switch
            {
                "None" => Return((PythonTypeSpec)PythonTypeSpec.None),
                "Any" => Return((PythonTypeSpec)PythonTypeSpec.Any),
                "int" => Return((PythonTypeSpec)PythonTypeSpec.Int),
                "str" => Return((PythonTypeSpec)PythonTypeSpec.Str),
                "float" => Return((PythonTypeSpec)PythonTypeSpec.Float),
                "bool" => Return((PythonTypeSpec)PythonTypeSpec.Bool),
                "bytes" => Return((PythonTypeSpec)PythonTypeSpec.Bytes),
                "Buffer" or "collections.abc.Buffer" => Return((PythonTypeSpec)PythonTypeSpec.Buffer),
                "Optional" or "typing.Optional" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (of) => new OptionalType(of)),
                "list" or "List" or "typing.List" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (of) => new ListType(of)),
                "Sequence" or "collections.abc.Sequence" or "typing.Sequence" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (of) => new SequenceType(of)),
                "dict" or "Dict" or "typing.Dict" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (k, v) => new DictType(k, v)),
                "Mapping" or "collections.abc.Mapping" or "typing.Mapping" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (k, v) => new MappingType(k, v)),
                "Generator" or "collections.abc.Generator" or "typing.Generator" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (y, s, r) => new GeneratorType(y, s, r)),
                "Coroutine" or "collections.abc.Coroutine" or "typing.Coroutine" =>
                    typeDefinitionParser.Subscript(PythonTypeSpec (y, s, r) => new CoroutineType(y, s, r)),
                "Callable" or "typing.Callable" or "collections.abc.Callable" =>
                    // > The subscription syntax must always be used with exactly two values: the argument list and the
                    // > return type. The argument list must be a list of types, a `ParamSpec`, `Concatenate`, or an
                    // > ellipsis. The return type must be a single type.
                    //
                    // Source: Annotating callable objects,
                    // <https://docs.python.org/3/library/typing.html#annotating-callable-objects>
                    from callable in
                        typeDefinitionParser.ManyDelimitedBy(comma)
                                            .Subscript()
                                            .ThenIgnore(comma)
                                            .Then(ps => from r in typeDefinitionParser
                                                        select (Parameters: ps, Return: r))
                                            .Subscript()
                    select (PythonTypeSpec)new CallbackType([..callable.Parameters], callable.Return),
                "Literal" or "typing.Literal" =>
                    // Literal can contain any PythonConstant, or a list of them.
                    // See PEP586 https://peps.python.org/pep-0586/
                    // It can also contain an expression, like a typedef but we don't support that yet.
                    // It could also contain another Literal, but we don't support that yet either.
                    from literals in
                        ConstantValueParser.AtLeastOnceDelimitedBy(comma)
                                           .Subscript()
                    select (PythonTypeSpec)new LiteralType([..literals]),
                "Union" or "typing.Union" =>
                    from subscript in
                        typeDefinitionParser.AtLeastOnceDelimitedBy(comma)
                                            .Subscript()
                    select UnionType.Normalize(subscript),
                "tuple" or "Tuple" or "typing.Tuple" =>
                    Parse.OneOf(from of in typeDefinitionParser.ThenIgnore(comma)
                                                               .ThenIgnore(Token.EqualTo(PythonToken.Ellipsis))
                                                               .Subscript()
                                                               .Try()
                                select (PythonTypeSpec)new VariadicTupleType(of),
                                from subscript in typeDefinitionParser.AtLeastOnceDelimitedBy(comma)
                                                                      .Subscript()
                                select (PythonTypeSpec)new TupleType([..subscript]))
                         .OptionalOrDefault(PythonTypeSpec.Tuple),
                var other =>
                    from subscript in
                        typeDefinitionParser.AtLeastOnceDelimitedBy(comma)
                                            .Subscript()
                                            .OptionalOrDefault([])
                    select (PythonTypeSpec)new ParsedPythonTypeSpec(other, [..subscript]),
            }
            select type;

        return Combinators.Named(name: "Type Definition", parser:
                   from ts in parser.AtLeastOnceDelimitedBy(Token.EqualTo(PythonToken.Pipe))
                   select ts switch
                   {
                       [var single] => single,
                       var multiple => UnionType.Normalize([..multiple])
                   });
    }

    private static TokenListParser<PythonToken, T> Return<T>(T value) => Parse.Return<PythonToken, T>(value);

    private static TokenListParser<PythonToken, T> Subscript<T>(this TokenListParser<PythonToken, T> parser) =>
        parser.Between(Token.EqualTo(PythonToken.OpenBracket), Token.EqualTo(PythonToken.CloseBracket));

    private static TokenListParser<PythonToken, TResult>
        Subscript<T, TResult>(this TokenListParser<PythonToken, T> parser,
                              Func<T, TResult> selector) =>
        parser.Subscript().Select(selector);

    private static TokenListParser<PythonToken, TResult>
        Subscript<T, TResult>(this TokenListParser<PythonToken, T> parser,
                              Func<T, T, TResult> selector) =>
        parser.ThenIgnore(Token.EqualTo(PythonToken.Comma))
              .Then(a => from b in parser select selector(a, b))
              .Subscript();

    private static TokenListParser<PythonToken, TResult>
        Subscript<T, TResult>(this TokenListParser<PythonToken, T> parser,
                              Func<T, T, T, TResult> selector) =>
        parser.ThenIgnore(Token.EqualTo(PythonToken.Comma))
              .Then(a => from b in parser select (a, b))
              .ThenIgnore(Token.EqualTo(PythonToken.Comma))
              .Then(ab => from c in parser select selector(ab.Item1, ab.Item2, c))
              .Subscript();
}
