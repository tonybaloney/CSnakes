using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using PythonTypeSpecParser = Superpower.TokenListParser<CSnakes.Parser.PythonToken, CSnakes.Parser.Types.PythonTypeSpec>;

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

    private static class TypeDefinitionSubParsers
    {
        private static readonly TokenListParser<PythonToken, Token<PythonToken>> Comma = Token.EqualTo(PythonToken.Comma);

        public static readonly PythonTypeSpecParser None = Return((PythonTypeSpec)PythonTypeSpec.None);
        public static readonly PythonTypeSpecParser Any = Return((PythonTypeSpec)PythonTypeSpec.Any);
        public static readonly PythonTypeSpecParser Int = Return((PythonTypeSpec)PythonTypeSpec.Int);
        public static readonly PythonTypeSpecParser Str = Return((PythonTypeSpec)PythonTypeSpec.Str);
        public static readonly PythonTypeSpecParser Float = Return((PythonTypeSpec)PythonTypeSpec.Float);
        public static readonly PythonTypeSpecParser Bool = Return((PythonTypeSpec)PythonTypeSpec.Bool);
        public static readonly PythonTypeSpecParser Bytes = Return((PythonTypeSpec)PythonTypeSpec.Bytes);
        public static readonly PythonTypeSpecParser Buffer = Return((PythonTypeSpec)PythonTypeSpec.Buffer);

        private static readonly PythonTypeSpecParser TypeDefinitionParser = Parse.Ref(() => PythonTypeDefinitionParser);

        public static readonly PythonTypeSpecParser Optional = TypeDefinitionParser.Subscript(PythonTypeSpec (of) => new OptionalType(of));
        public static readonly PythonTypeSpecParser List = TypeDefinitionParser.Subscript(PythonTypeSpec (of) => new ListType(of));
        public static readonly PythonTypeSpecParser Sequence = TypeDefinitionParser.Subscript(PythonTypeSpec (of) => new SequenceType(of));
        public static readonly PythonTypeSpecParser Dict = TypeDefinitionParser.Subscript(PythonTypeSpec (k, v) => new DictType(k, v));
        public static readonly PythonTypeSpecParser Mapping = TypeDefinitionParser.Subscript(PythonTypeSpec (k, v) => new MappingType(k, v));
        public static readonly PythonTypeSpecParser Generator = TypeDefinitionParser.Subscript(PythonTypeSpec (y, s, r) => new GeneratorType(y, s, r));
        public static readonly PythonTypeSpecParser Coroutine = TypeDefinitionParser.Subscript(PythonTypeSpec (y, s, r) => new CoroutineType(y, s, r));

        public static readonly PythonTypeSpecParser Callable =
            //
            // > The subscription syntax must always be used with exactly two values: the argument list and the
            // > return type. The argument list must be a list of types, a `ParamSpec`, `Concatenate`, or an
            // > ellipsis. The return type must be a single type.
            //
            // Source: Annotating callable objects,
            // <https://docs.python.org/3/library/typing.html#annotating-callable-objects>
            //
            // NOTE: This parser only supports the form of `Callable` where the first value is a list of
            // types, like `Callable[[int, str], bool]`.
            //
            from callable in
                TypeDefinitionParser.ManyDelimitedBy(Comma)
                                    .Subscript()
                                    .ThenIgnore(Comma)
                                    .Then(ps => from r in TypeDefinitionParser
                                                select (Parameters: ps, Return: r))
                                    .Subscript()
            select (PythonTypeSpec)new CallableType([..callable.Parameters], callable.Return);

        public static readonly PythonTypeSpecParser Literal =
            // Literal can contain any PythonConstant, or a list of them.
            // See PEP586 https://peps.python.org/pep-0586/
            // It can also contain an expression, like a typedef but we don't support that yet.
            // It could also contain another Literal, but we don't support that yet either.
            from literals in
                ConstantValueParser.AtLeastOnceDelimitedBy(Comma)
                    .Subscript()
            select (PythonTypeSpec)new LiteralType([..literals]);

        public static readonly PythonTypeSpecParser Tuple =
            Parse.OneOf(from of in TypeDefinitionParser.ThenIgnore(Comma)
                                                       .ThenIgnore(Token.EqualTo(PythonToken.Ellipsis))
                                                       .Subscript()
                                                       .Try()
                        select (PythonTypeSpec)new VariadicTupleType(of),
                        from subscript in TypeDefinitionParser.AtLeastOnceDelimitedBy(Comma)
                                                              .Subscript()
                        select (PythonTypeSpec)new TupleType([..subscript]))
                 .OptionalOrDefault(PythonTypeSpec.Tuple);

        public static readonly PythonTypeSpecParser Union =
            from subscript in
                TypeDefinitionParser.AtLeastOnceDelimitedBy(Comma)
                                    .Subscript()
            select UnionType.Normalize(subscript);

        public static readonly TokenListParser<PythonToken, PythonTypeSpec[]> Subscript =
            TypeDefinitionParser.AtLeastOnceDelimitedBy(Comma)
                                .Subscript()
                                .OptionalOrDefault([]);
    }

    private static TokenListParser<PythonToken, PythonTypeSpec> CreatePythonTypeDefinitionParser()
    {
        var parser =
            from name in Parse.OneOf(Token.EqualTo(PythonToken.None),
                                     Token.EqualTo(PythonToken.Identifier),
                                     Token.EqualTo(PythonToken.QualifiedIdentifier))
            from type in name.ToStringValue() switch
            {
                "None"                                                           => TypeDefinitionSubParsers.None,
                "Any"                                                            => TypeDefinitionSubParsers.Any,
                "int"                                                            => TypeDefinitionSubParsers.Int,
                "str"                                                            => TypeDefinitionSubParsers.Str,
                "float"                                                          => TypeDefinitionSubParsers.Float,
                "bool"                                                           => TypeDefinitionSubParsers.Bool,
                "bytes"                                                          => TypeDefinitionSubParsers.Bytes,
                "Buffer" or "collections.abc.Buffer"                             => TypeDefinitionSubParsers.Buffer,
                "Optional" or "typing.Optional"                                  => TypeDefinitionSubParsers.Optional,
                "list" or "List" or "typing.List"                                => TypeDefinitionSubParsers.List,
                "Sequence" or "collections.abc.Sequence" or "typing.Sequence"    => TypeDefinitionSubParsers.Sequence,
                "dict" or "Dict" or "typing.Dict"                                => TypeDefinitionSubParsers.Dict,
                "Mapping" or "collections.abc.Mapping" or "typing.Mapping"       => TypeDefinitionSubParsers.Mapping,
                "Generator" or "collections.abc.Generator" or "typing.Generator" => TypeDefinitionSubParsers.Generator,
                "Coroutine" or "collections.abc.Coroutine" or "typing.Coroutine" => TypeDefinitionSubParsers.Coroutine,
                "Callable" or "typing.Callable" or "collections.abc.Callable"    => TypeDefinitionSubParsers.Callable,
                "Literal" or "typing.Literal"                                    => TypeDefinitionSubParsers.Literal,
                "Union" or "typing.Union"                                        => TypeDefinitionSubParsers.Union,
                "tuple" or "Tuple" or "typing.Tuple"                             => TypeDefinitionSubParsers.Tuple,

                var other => from subscript in TypeDefinitionSubParsers.Subscript
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
