using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace PythonSourceGenerator.Parser;
public static class PythonSignatureTokenizer
{
    public static Tokenizer<PythonSignatureTokens.PythonSignatureToken> Instance { get; } =
        new TokenizerBuilder<PythonSignatureTokens.PythonSignatureToken>()
        .Ignore(Span.WhiteSpace)
        .Match(Character.EqualTo('('), PythonSignatureTokens.PythonSignatureToken.OpenParenthesis)
        .Match(Character.EqualTo(')'), PythonSignatureTokens.PythonSignatureToken.CloseParenthesis)
        .Match(Character.EqualTo('['), PythonSignatureTokens.PythonSignatureToken.OpenBracket)
        .Match(Character.EqualTo(']'), PythonSignatureTokens.PythonSignatureToken.CloseBracket)
        .Match(Character.EqualTo(':'), PythonSignatureTokens.PythonSignatureToken.Colon)
        .Match(Character.EqualTo(','), PythonSignatureTokens.PythonSignatureToken.Comma)
        .Match(Character.EqualTo('*').IgnoreThen(Character.EqualTo('*')), PythonSignatureTokens.PythonSignatureToken.DoubleAsterisk)
        .Match(Character.EqualTo('*'), PythonSignatureTokens.PythonSignatureToken.Asterisk)
        .Match(Character.EqualTo('-').IgnoreThen(Character.EqualTo('>')), PythonSignatureTokens.PythonSignatureToken.Arrow)
        .Match(Character.EqualTo('/'), PythonSignatureTokens.PythonSignatureToken.Slash)
        .Match(Character.EqualTo('='), PythonSignatureTokens.PythonSignatureToken.Equal)
        .Match(Span.EqualTo("def"), PythonSignatureTokens.PythonSignatureToken.Def, requireDelimiters: true)
        .Match(Span.EqualTo("async"), PythonSignatureTokens.PythonSignatureToken.Async, requireDelimiters: true)
        .Match(Span.EqualTo("..."), PythonSignatureTokens.PythonSignatureToken.Ellipsis)
        .Match(Span.EqualTo("None"), PythonSignatureTokens.PythonSignatureToken.None, requireDelimiters: true)
        .Match(Span.EqualTo("True"), PythonSignatureTokens.PythonSignatureToken.True, requireDelimiters: true)
        .Match(Span.EqualTo("False"), PythonSignatureTokens.PythonSignatureToken.False, requireDelimiters: true)
        .Match(Identifier.CStyle, PythonSignatureTokens.PythonSignatureToken.Identifier, requireDelimiters: true) // TODO: Does this require delimiters?
        .Match(PythonSignatureParser.IntegerConstantToken, PythonSignatureTokens.PythonSignatureToken.Integer, requireDelimiters: true)
        .Match(PythonSignatureParser.DecimalConstantToken, PythonSignatureTokens.PythonSignatureToken.Decimal, requireDelimiters: true)
        .Match(PythonSignatureParser.DoubleQuotedStringConstantToken, PythonSignatureTokens.PythonSignatureToken.DoubleQuotedString)
        .Match(PythonSignatureParser.SingleQuotedStringConstantToken, PythonSignatureTokens.PythonSignatureToken.SingleQuotedString)
        .Build();
}
