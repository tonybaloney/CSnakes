using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonParameterType> PythonStarArgTokenizer { get; } =
        (from star in Token.EqualTo(PythonToken.Asterisk)
         from name in Token.EqualTo(PythonToken.Identifier).Optional()
         select new PythonParameterType(name.HasValue ? name.Value.ToStringValue() : "args", PythonFunctionParameterType.Star))
        .Named("Star Arg");

    public static TokenListParser<PythonToken, PythonParameterType> PythonDoubleStarArgTokenizer { get; } =
        (from star in Token.EqualTo(PythonToken.DoubleAsterisk)
         from name in Token.EqualTo(PythonToken.Identifier)
         select new PythonParameterType(name.ToStringValue(), PythonFunctionParameterType.DoubleStar))
        .Named("Double Star Arg");

    public static TokenListParser<PythonToken, PythonParameterType> PythonNormalArgTokenizer { get; } =
        (from name in Token.EqualTo(PythonToken.Identifier)
         select new PythonParameterType(name.ToStringValue(), PythonFunctionParameterType.Normal))
        .Named("Normal Arg");

    public static TokenListParser<PythonToken, PythonParameterType?> PythonArgTokenizer { get; } =
        PythonStarArgTokenizer.AsNullable()
        .Or(PythonDoubleStarArgTokenizer.AsNullable())
        .Or(PythonNormalArgTokenizer.AsNullable())
        .Named("Arg");
}
