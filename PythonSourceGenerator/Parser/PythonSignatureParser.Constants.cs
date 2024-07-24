using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Globalization;

namespace PythonSourceGenerator.Parser;
public static partial class PythonSignatureParser
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
            from rest in Character.Digit.Or(Character.In('.', 'e', 'E', '+', '-')).IgnoreMany()
            select Unit.Value;

    public static TextParser<Unit> HexidecimalConstantToken { get; } =
        from prefix in Span.EqualTo("0x")
        from digits in Character.HexDigit.AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> BinaryConstantToken { get; } =
        from prefix in Span.EqualTo("0b")
        from digits in Character.EqualTo('0').Or(Character.EqualTo('1')).AtLeastOnce()
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

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> DoubleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.DoubleQuotedString)
        .Apply(ConstantParsers.DoubleQuotedString)
        .Select(s => new PythonConstant(s))
        .Named("Double Quoted String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> SingleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.SingleQuotedString)
        .Apply(ConstantParsers.SingleQuotedString)
        .Select(s => new PythonConstant(s))
        .Named("Single Quoted String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> DecimalConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Decimal)
        .Apply(ConstantParsers.DecimalDoubleWithExponents)
        .Select(d => new PythonConstant(d))
        .Named("Decimal Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> IntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Integer)
        .Apply(ConstantParsers.IntegerInt64)
        .Select(d => new PythonConstant(d))
        .Named("Integer Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> HexidecimalIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.HexidecimalInteger)
        .Apply(ConstantParsers.HexidecimalConstantParser)
        .Select(d => new PythonConstant { Type = PythonConstant.ConstantType.HexidecimalInteger, IntegerValue = (long)d })
        .Named("Hexidecimal Integer Constant");
    
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> BinaryIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.BinaryInteger)
        .Apply(ConstantParsers.BinaryConstantParser)
        .Select(d => new PythonConstant { Type = PythonConstant.ConstantType.BinaryInteger, IntegerValue = (long)d })
        .Named("Binary Integer Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> BoolConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.True).Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.False))
        .Select(d => new PythonConstant(d.Kind == PythonSignatureTokens.PythonSignatureToken.True ))
        .Named("Bool Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> NoneConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.None)
        .Select(d => new PythonConstant { Type = PythonConstant.ConstantType.None })
        .Named("None Constant");

    // Any constant value
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant?> ConstantValueTokenizer { get; } =
        DecimalConstantTokenizer.AsNullable()
        .Or(IntegerConstantTokenizer.AsNullable())
        .Or(HexidecimalIntegerConstantTokenizer.AsNullable())
        .Or(BinaryIntegerConstantTokenizer.AsNullable())
        .Or(BoolConstantTokenizer.AsNullable())
        .Or(NoneConstantTokenizer.AsNullable())
        .Or(DoubleQuotedStringConstantTokenizer.AsNullable())
        .Or(SingleQuotedStringConstantTokenizer.AsNullable())
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

        public static TextParser<UInt64> HexidecimalConstantParser { get; } =
            from prefix in Span.EqualTo("0x")
            from digits in Character.HexDigit.AtLeastOnce()
            select UInt64.Parse(new string(digits), System.Globalization.NumberStyles.HexNumber);

        public static TextParser<UInt64> BinaryConstantParser { get; } =
            from prefix in Span.EqualTo("0b")
            // TODO: Consider Binary Format specifier introduced in .NET 8 https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#binary-format-specifier-b
            from digits in Character.EqualTo('0').Or(Character.EqualTo('1')).AtLeastOnce()
            select Convert.ToUInt64(new string(digits), 2);

        // Based on Numerics.IntegerInt64 but with Python characteristics (underscores after the first digit)
        public static TextParser<long> IntegerInt64 { get; } = delegate (TextSpan input)
        {
            bool negative = false;
            Result<char> result = input.ConsumeChar();
            if (!result.HasValue)
            {
                return Result.Empty<long>(input, ["sign", "digit"]);
            }
            if (result.Value == '-')
            {
                negative = true;
                result = result.Remainder.ConsumeChar();
            }
            else if (result.Value == '+')
            {
                result = result.Remainder.ConsumeChar();
            }
            if (!result.HasValue || !char.IsDigit(result.Value))
            {
                return Result.Empty<long>(input, ["digit"]);
            }
            long num = 0L;

            TextSpan remainder ;
            do
            {
                if (result.Value != '_')
                {
                    num = 10 * num + (result.Value - 48);
                }
                remainder = result.Remainder;
                result = remainder.ConsumeChar();
            }
            while (result.HasValue && (char.IsDigit(result.Value) || result.Value == '_'));

            if (negative) num = -num;

            return Result.Value(num, input, remainder);
        };

        public static TextParser<double> DecimalDoubleWithExponents { get; } = Numerics.Decimal.Select((TextSpan span) => double.Parse(span.ToStringValue(), NumberStyles.Float, CultureInfo.InvariantCulture));
    }
}
