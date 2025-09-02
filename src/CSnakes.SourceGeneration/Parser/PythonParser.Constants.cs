using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Globalization;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    static readonly char[] pythonStringPrefixes = ['u', 'U'];
    static readonly char[] pythonByteStringPrefixes = ['b', 'B'];

    public static TextParser<char> UnderScoreOrDigit { get; } =
        Character.Matching(char.IsDigit, "digit").Or(Character.EqualTo('_'));

    public static TextParser<Unit> IntegerConstantToken { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from firstdigit in Character.Digit
        from digits in UnderScoreOrDigit.Many().OptionalOrDefault([])
        select Unit.Value;

    public static TextParser<Unit> DecimalConstantToken { get; } =
            from sign in Character.EqualTo('-').OptionalOrDefault()
            from first in Character.Digit
            from rest in UnderScoreOrDigit.Or(Character.In('.', 'e', 'E', '+', '-')).IgnoreMany()
            select Unit.Value;

    public static TextParser<Unit> HexadecimalConstantToken { get; } =
        from prefix in Span.EqualTo("0x")
        from digits in Character.EqualTo('_').Or(Character.HexDigit).AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> BinaryConstantToken { get; } =
        from prefix in Span.EqualTo("0b")
        from digits in Character.In('0', '1', '_').AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> OctalConstantToken { get; } =
        from prefix in Span.EqualTo("0o")
        from digits in Character.In('0', '1', '2', '3', '4', '5', '6', '7', '_').AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> DoubleQuotedStringConstantToken { get; } =
        from prefix in Character.In(pythonStringPrefixes).OptionalOrDefault() // TODO: support raw literals
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select Unit.Value;

    public static TextParser<Unit> SingleQuotedStringConstantToken { get; } =
        from prefix in Character.In(pythonStringPrefixes).OptionalOrDefault()
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select Unit.Value;

    public static TextParser<Unit> DoubleQuotedByteStringConstantToken { get; } =
        from prefix in Character.In(pythonByteStringPrefixes) // TODO: support raw literals
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select Unit.Value;

    public static TextParser<Unit> SingleQuotedByteStringConstantToken { get; } =
        from prefix in Character.In(pythonByteStringPrefixes)
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select Unit.Value;

    public static TokenListParser<PythonToken, PythonConstant.String> DoubleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.DoubleQuotedString)
        .Apply(ConstantParsers.DoubleQuotedString)
        .Named("Double Quoted String Constant");

    public static TokenListParser<PythonToken, PythonConstant.String> SingleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.SingleQuotedString)
        .Apply(ConstantParsers.SingleQuotedString)
        .Named("Single Quoted String Constant");

    public static TokenListParser<PythonToken, PythonConstant.ByteString> DoubleQuotedByteStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.DoubleQuotedByteString)
        .Apply(ConstantParsers.DoubleQuotedByteString)
        .Named("Double Quoted Byte String Constant");

    public static TokenListParser<PythonToken, PythonConstant.ByteString> SingleQuotedByteStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.SingleQuotedByteString)
        .Apply(ConstantParsers.SingleQuotedByteString)
        .Named("Single Quoted Byte String Constant");

    public static TokenListParser<PythonToken, PythonConstant.Float> DecimalConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Decimal)
        .Select(token => new PythonConstant.Float(double.Parse(token.ToStringValue().Replace("_", ""), NumberStyles.Float, CultureInfo.InvariantCulture)))
        .Named("Decimal Constant");

    public static TokenListParser<PythonToken, PythonConstant.Integer> IntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Integer)
        .Select(d => new PythonConstant.Integer(long.Parse(d.ToStringValue().Replace("_", ""), NumberStyles.Integer)))
        .Named("Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant.HexadecimalInteger> HexadecimalIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.HexadecimalInteger)
        .Select(d => new PythonConstant.HexadecimalInteger(long.Parse(d.ToStringValue().Substring(2).Replace("_", ""), NumberStyles.HexNumber)))
        .Named("Hexadecimal Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant.HexadecimalInteger> OctalIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.OctalInteger)
        .Select(d => new PythonConstant.HexadecimalInteger(PythonParserConstants.ParseOctal(d.ToStringValue().Substring(2))))
        .Named("Octal Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant.BinaryInteger> BinaryIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.BinaryInteger)
        // TODO: Consider Binary Format specifier introduced in .NET 8 https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#binary-format-specifier-b
        .Select(d => new PythonConstant.BinaryInteger((long)Convert.ToUInt64(d.ToStringValue().Substring(2).Replace("_", ""), 2)))
        .Named("Binary Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant.Bool> BoolConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.True).Or(Token.EqualTo(PythonToken.False))
        .Select(d => (d.Kind == PythonToken.True ? PythonConstant.Bool.True : PythonConstant.Bool.False))
        .Named("Bool Constant");

    public static TokenListParser<PythonToken, PythonConstant.None> NoneConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.None)
        .Select(d => PythonConstant.None.Value)
        .Named("None Constant");

    public static TokenListParser<PythonToken, PythonConstant.Ellipsis> EllipsisConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Ellipsis)
        .Select(d => PythonConstant.Ellipsis.Value)
        .Named("Ellipsis (unspecified) Constant");

    // Any constant value
    public static TokenListParser<PythonToken, PythonConstant> ConstantValueParser { get; }

    private static TokenListParser<PythonToken, PythonConstant> CreateConstantValueParser() =>
        DecimalConstantTokenizer.AsBase()
        .Or(IntegerConstantTokenizer.AsBase())
        .Or(HexadecimalIntegerConstantTokenizer.AsBase())
        .Or(BinaryIntegerConstantTokenizer.AsBase())
        .Or(OctalIntegerConstantTokenizer.AsBase())
        .Or(BoolConstantTokenizer.AsBase())
        .Or(NoneConstantTokenizer.AsBase())
        .Or(EllipsisConstantTokenizer.AsBase())
        .Or(DoubleQuotedStringConstantTokenizer.AsBase())
        .Or(SingleQuotedStringConstantTokenizer.AsBase())
        .Or(DoubleQuotedByteStringConstantTokenizer.AsBase())
        .Or(SingleQuotedByteStringConstantTokenizer.AsBase())
        .Named("Constant");

    static class ConstantParsers
    {
        private static readonly TextParser<char> escapeString = Character.EqualTo('\\')
                            .IgnoreThen(
                                Character.EqualTo('\\')
                                .Or(Character.EqualTo('"'))
                                .Named("escape sequence"));

        private static readonly TextParser<char> singleQuoteEscapeString = Character.EqualTo('\\')
                            .IgnoreThen(
                                Character.EqualTo('\\')
                                .Or(Character.EqualTo('"'))
                                .Named("escape sequence"));

        public static TextParser<PythonConstant.String> DoubleQuotedString { get; } =
            from prefix in Character.In(pythonStringPrefixes).Optional() // TODO : support raw literals
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(escapeString)
                .Many()
            from close in Character.EqualTo('"')
            select new PythonConstant.String(new string(chars));

        public static TextParser<PythonConstant.ByteString> DoubleQuotedByteString { get; } =
            from prefix in Character.In(pythonByteStringPrefixes) // TODO : support raw literals
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(escapeString)
                .Many()
            from close in Character.EqualTo('"')
            select new PythonConstant.ByteString(new string(chars));

        public static TextParser<PythonConstant.String> SingleQuotedString { get; } =
            from prefix in Character.In(pythonStringPrefixes).Optional() // TODO : support raw literals
            from open in Character.EqualTo('\'')
            from chars in Character.ExceptIn('\'', '\\')
                .Or(singleQuoteEscapeString)
                .Many()
            from close in Character.EqualTo('\'')
            select new PythonConstant.String(new string(chars));

        public static TextParser<PythonConstant.ByteString> SingleQuotedByteString { get; } =
            from prefix in Character.In(pythonByteStringPrefixes) // TODO : support raw literals
            from open in Character.EqualTo('\'')
            from chars in Character.ExceptIn('\'', '\\')
                .Or(singleQuoteEscapeString)
                .Many()
            from close in Character.EqualTo('\'')
            select new PythonConstant.ByteString(new string(chars));
    }
}

file static class Extensions
{
    public static TokenListParser<TKind, PythonConstant> AsBase<TKind, T>(this TokenListParser<TKind, T> parser)
        where T : PythonConstant =>
        parser.Cast<TKind, T, PythonConstant>();
}

public static class PythonParserConstants
{
    /// <summary>
    /// Parses an octal string (e.g., "123") into a long integer.
    /// Ignores underscores in the string.
    /// </summary>
    /// <param name="octal">The octal string to parse.</param>
    /// <returns>The parsed long integer value.</returns>
    public static long ParseOctal(string octal)
    {
        long result = 0;
        foreach (char c in octal)
        {
            if (c == '_') continue; // Ignore underscores
            if (c < '0' || c > '7') throw new ArgumentException("Invalid octal character.");
            result = (result * 8) + (c - '0');
        }
        return result;
    }
}
