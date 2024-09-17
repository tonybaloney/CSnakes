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

    public static TokenListParser<PythonToken, PythonTypeSpec> PythonTypeNameTokenizer { get; } =
        (from name in Token.EqualTo(PythonToken.Identifier).Or(Token.EqualTo(PythonToken.None))
         select new PythonTypeSpec(name.ToStringValue(), [])).Named("Type Name");

    public static TokenListParser<PythonToken, PythonTypeSpec> PythonTypeArgumentsTokenizer { get; } =
        (from openBracket in Token.EqualTo(PythonToken.OpenBracket)
         from argumentCollection in
                PythonTypeDefinitionTokenizer
                    .AssumeNotNull()
                    .ManyDelimitedBy(
                        Token.EqualTo(PythonToken.Comma)
                    )
         from closeBracket in Token.EqualTo(PythonToken.CloseBracket)
         select new PythonTypeSpec("collection", argumentCollection)).Named("Type collection");

    public static TokenListParser<PythonToken, PythonTypeSpec> PythonTypeNameWithArgumentsTokenizer { get; } =
        (from name in Token.EqualTo(PythonToken.Identifier)
         from openBracket in Token.EqualTo(PythonToken.OpenBracket)
         from argumentCollection in
                PythonTypeDefinitionTokenizer
                    .AssumeNotNull()
                    .ManyDelimitedBy(
                        Token.EqualTo(PythonToken.Comma)
                    )
         from closeBracket in Token.EqualTo(PythonToken.CloseBracket)
         select new PythonTypeSpec(name.ToStringValue(), argumentCollection)).Named("Type with arguments");

    /// <summary>
    /// Can be :
    ///  1. a name, e.g. int, str, bool, etc.
    ///  2. a generic type, e.g. list[int], tuple[str, int], etc.
    ///  3. a list of parameters, e.g. type[ [int, str, bool], int]
    /// </summary>
    public static TokenListParser<PythonToken, PythonTypeSpec?> PythonTypeDefinitionTokenizer { get; } =
        PythonTypeNameWithArgumentsTokenizer.AsNullable()
        //.Or(PythonTypeArgumentsTokenizer)
        .Or(PythonTypeNameTokenizer.AsNullable())
        .Named("Type Definition");
}
