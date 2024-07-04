using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;
using System.Collections.Generic;

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
        .Match(Character.EqualTo('*'), PythonSignatureTokens.PythonSignatureToken.Asterisk)
        .Match(Character.EqualTo('*').IgnoreThen(Character.EqualTo('*')), PythonSignatureTokens.PythonSignatureToken.DoubleAsterisk)
        .Match(Character.EqualTo('-').IgnoreThen(Character.EqualTo('>')), PythonSignatureTokens.PythonSignatureToken.Arrow)
        .Match(Character.EqualTo('/'), PythonSignatureTokens.PythonSignatureToken.Slash)
        .Match(Character.EqualTo('='), PythonSignatureTokens.PythonSignatureToken.Equal)
        .Match(Span.EqualTo("None"), PythonSignatureTokens.PythonSignatureToken.None)
        .Match(Span.EqualTo("def"), PythonSignatureTokens.PythonSignatureToken.Def)
        .Match(Span.EqualTo("async"), PythonSignatureTokens.PythonSignatureToken.Async)
        .Match(Span.EqualTo("..."), PythonSignatureTokens.PythonSignatureToken.Ellipsis)
        .Match(Identifier.CStyle, PythonSignatureTokens.PythonSignatureToken.Identifier)
        .Match(Numerics.Natural, PythonSignatureTokens.PythonSignatureToken.Number)
        .Build();


    public static TokenList<PythonSignatureTokens.PythonSignatureToken> Tokenize(string input)
        {
        var tokenizer = Instance.TryTokenize(input);
        if (tokenizer.HasValue)
        {
            return tokenizer.Value;
        }
        else
        {
            throw new System.Exception(tokenizer.ErrorMessage);
        }
    }
}
