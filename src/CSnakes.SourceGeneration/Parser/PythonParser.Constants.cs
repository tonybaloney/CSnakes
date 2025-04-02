using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Globalization;

namespace CSnakes.Parser;
public static partial class PythonParser
{
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

    public static TextParser<Unit> HexidecimalConstantToken { get; } =
        from prefix in Span.EqualTo("0x")
        from digits in Character.EqualTo('_').Or(Character.HexDigit).AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> BinaryConstantToken { get; } =
        from prefix in Span.EqualTo("0b")
        from digits in Character.In('0', '1', '_').AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> DoubleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select Unit.Value;

    public static TextParser<Unit> SingleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select Unit.Value;

    public static TokenListParser<PythonToken, PythonConstant.String> DoubleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.DoubleQuotedString)
        .Apply(ConstantParsers.DoubleQuotedString)
        .Select(s => new PythonConstant.String(s))
        .Named("Double Quoted String Constant");

    public static TokenListParser<PythonToken, PythonConstant.String> SingleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.SingleQuotedString)
        .Apply(ConstantParsers.SingleQuotedString)
        .Select(s => new PythonConstant.String(s))
        .Named("Single Quoted String Constant");

    public static TokenListParser<PythonToken, PythonConstant.Float> DecimalConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Decimal)
        .Select(token => new PythonConstant.Float(double.Parse(token.ToStringValue().Replace("_", ""), NumberStyles.Float, CultureInfo.InvariantCulture)))
        .Named("Decimal Constant");

    public static TokenListParser<PythonToken, PythonConstant.Integer> IntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Integer)
        .Select(d => new PythonConstant.Integer(long.Parse(d.ToStringValue().Replace("_", ""), NumberStyles.Integer)))
        .Named("Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant.HexidecimalInteger> HexidecimalIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.HexidecimalInteger)
        .Select(d => new PythonConstant.HexidecimalInteger(long.Parse(d.ToStringValue().Substring(2).Replace("_", ""), NumberStyles.HexNumber)))
        .Named("Hexidecimal Integer Constant");

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

    // Any constant value
    public static TokenListParser<PythonToken, PythonConstant?> ConstantValueTokenizer { get; } =
        DecimalConstantTokenizer.AsBase().AsNullable()
        .Or(IntegerConstantTokenizer.AsBase().AsNullable())
        .Or(HexidecimalIntegerConstantTokenizer.AsBase().AsNullable())
        .Or(BinaryIntegerConstantTokenizer.AsBase().AsNullable())
        .Or(BoolConstantTokenizer.AsBase().AsNullable())
        .Or(NoneConstantTokenizer.AsBase().AsNullable())
        .Or(DoubleQuotedStringConstantTokenizer.AsBase().AsNullable())
        .Or(SingleQuotedStringConstantTokenizer.AsBase().AsNullable())
        .Named("Constant");

    static class ConstantParsers
    {
        public static TextParser<string> DoubleQuotedString { get; } =
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('"'))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('"')
            select new string(chars);

        public static TextParser<string> SingleQuotedString { get; } =
            from open in Character.EqualTo('\'')
            from chars in Character.ExceptIn('\'', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('\''))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('\'')
            select new string(chars);
    }
}

file static class Extensions
{
    public static TokenListParser<TKind, PythonConstant> AsBase<TKind, T>(this TokenListParser<TKind, T> parser)
        where T : PythonConstant =>
        parser.Cast<TKind, T, PythonConstant>();
}
