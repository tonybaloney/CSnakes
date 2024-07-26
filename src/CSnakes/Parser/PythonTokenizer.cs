﻿using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace PythonSourceGenerator.Parser;
public static class PythonTokenizer
{
    public static Tokenizer<PythonToken> Instance { get; } =
        new TokenizerBuilder<PythonToken>()
        .Ignore(Span.WhiteSpace)
        .Ignore(Comment.ShellStyle)
        .Match(Character.EqualTo('('), PythonToken.OpenParenthesis)
        .Match(Character.EqualTo(')'), PythonToken.CloseParenthesis)
        .Match(Character.EqualTo('['), PythonToken.OpenBracket)
        .Match(Character.EqualTo(']'), PythonToken.CloseBracket)
        .Match(Character.EqualTo(':'), PythonToken.Colon)
        .Match(Character.EqualTo(','), PythonToken.Comma)
        .Match(Character.EqualTo('*').IgnoreThen(Character.EqualTo('*')), PythonToken.DoubleAsterisk)
        .Match(Character.EqualTo('*'), PythonToken.Asterisk)
        .Match(Character.EqualTo('-').IgnoreThen(Character.EqualTo('>')), PythonToken.Arrow)
        .Match(Character.EqualTo('/'), PythonToken.Slash)
        .Match(Character.EqualTo('='), PythonToken.Equal)
        .Match(Span.EqualTo("def"), PythonToken.Def, requireDelimiters: true)
        .Match(Span.EqualTo("async"), PythonToken.Async, requireDelimiters: true)
        .Match(Span.EqualTo("..."), PythonToken.Ellipsis)
        .Match(Span.EqualTo("None"), PythonToken.None, requireDelimiters: true)
        .Match(Span.EqualTo("True"), PythonToken.True, requireDelimiters: true)
        .Match(Span.EqualTo("False"), PythonToken.False, requireDelimiters: true)
        .Match(Identifier.CStyle, PythonToken.Identifier, requireDelimiters: true) // TODO: (track) Does this require delimiters?
        .Match(PythonParser.IntegerConstantToken, PythonToken.Integer, requireDelimiters: true)
        .Match(PythonParser.DecimalConstantToken, PythonToken.Decimal, requireDelimiters: true)
        .Match(PythonParser.HexidecimalConstantToken, PythonToken.HexidecimalInteger, requireDelimiters: true)
        .Match(PythonParser.BinaryConstantToken, PythonToken.BinaryInteger, requireDelimiters: true)
        .Match(PythonParser.DoubleQuotedStringConstantToken, PythonToken.DoubleQuotedString)
        .Match(PythonParser.SingleQuotedStringConstantToken, PythonToken.SingleQuotedString)
        .Build();
}