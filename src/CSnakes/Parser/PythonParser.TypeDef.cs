using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TextParser<TextSpan> QualifiedName { get; } = 
        Span.MatchedBy(
            Character.Letter.Or(Character.EqualTo('_'))
            .IgnoreThen(Character.LetterOrDigit.Or(Character.EqualTo('_')).Many())
            .AtLeastOnceDelimitedBy(Character.EqualTo('.'))
        );

    public static TokenListParser<PythonToken, PythonTypeSpec?> PythonTypeDefinitionTokenizer { get; } =
        (from name in Token.EqualTo(PythonToken.Identifier).Or(Token.EqualTo(PythonToken.None))
#pragma warning disable CS8620
         from openBracket in Token.EqualTo(PythonToken.OpenBracket)
            .Then(_ =>
                PythonTypeDefinitionTokenizer
                    .AssumeNotNull()
                    .ManyDelimitedBy(
                        Token.EqualTo(PythonToken.Comma),
                        Token.EqualTo(PythonToken.CloseBracket)
                    )
            )
#pragma warning restore CS8620
            .OptionalOrDefault()
         select new PythonTypeSpec(name.ToStringValue(), openBracket))
        .Named("Type Definition");
}
