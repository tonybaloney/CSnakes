using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonParameterType> PythonStarArgParser { get; } =
        (from star in Token.EqualTo(PythonToken.Asterisk)
         from name in Token.EqualTo(PythonToken.Identifier).Optional()
         select new PythonParameterType(name.HasValue ? name.Value.ToStringValue() : "args", PythonFunctionParameterType.Star))
        .Named("Star Arg");

    public static TokenListParser<PythonToken, PythonParameterType> PythonDoubleStarArgParser { get; } =
        (from star in Token.EqualTo(PythonToken.DoubleAsterisk)
         from name in Token.EqualTo(PythonToken.Identifier)
         select new PythonParameterType(name.ToStringValue(), PythonFunctionParameterType.DoubleStar))
        .Named("Double Star Arg");

    public static TokenListParser<PythonToken, PythonParameterType> PythonNormalArgParser { get; } =
        (from name in Token.EqualTo(PythonToken.Identifier)
         select new PythonParameterType(name.ToStringValue(), PythonFunctionParameterType.Normal))
        .Named("Normal Arg");

    public static TokenListParser<PythonToken, PythonParameterType?> PythonArgParser { get; } =
        PythonStarArgParser.AsNullable()
        .Or(PythonDoubleStarArgParser.AsNullable())
        .Or(PythonNormalArgParser.AsNullable())
        .Named("Arg");
}
