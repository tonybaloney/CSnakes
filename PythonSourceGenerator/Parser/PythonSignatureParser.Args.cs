using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace PythonSourceGenerator.Parser;
public static partial class PythonSignatureParser
{
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonParameterType> PythonStarArgTokenizer { get; } =
        (from star in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Asterisk)
         from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier).Optional()
         select new PythonParameterType(name.HasValue ? name.Value.ToStringValue() : "args", PythonFunctionParameterType.Star))
        .Named("Star Arg");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonParameterType> PythonDoubleStarArgTokenizer { get; } =
        (from star in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.DoubleAsterisk)
         from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
         select new PythonParameterType(name.ToStringValue(), PythonFunctionParameterType.DoubleStar))
        .Named("Double Star Arg");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonParameterType> PythonNormalArgTokenizer { get; } =
        (from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
         select new PythonParameterType(name.ToStringValue(), PythonFunctionParameterType.Normal))
        .Named("Normal Arg");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonParameterType?> PythonArgTokenizer { get; } =
        PythonStarArgTokenizer.AsNullable()
        .Or(PythonDoubleStarArgTokenizer.AsNullable())
        .Or(PythonNormalArgTokenizer.AsNullable())
        .Named("Arg");
}
