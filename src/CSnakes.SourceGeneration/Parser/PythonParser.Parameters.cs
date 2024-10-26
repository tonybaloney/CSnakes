using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonFunctionParameter> PositionalOnlyParameterParser { get; } =
        Token.EqualTo(PythonToken.Slash)
        .Select(d => new PythonFunctionParameter("/", null, null, PythonFunctionParameterType.Slash))
        .Named("Positional Only Signal");

    public static TokenListParser<PythonToken, PythonFunctionParameter> PythonParameterTokenizer { get; } =
        (from arg in PythonArgTokenizer
         from type in Token.EqualTo(PythonToken.Colon).Optional().Then(
                _ => PythonTypeDefinitionTokenizer.AssumeNotNull().OptionalOrDefault()
             )
         from defaultValue in Token.EqualTo(PythonToken.Equal).Optional().Then(
                 _ => ConstantValueTokenizer.AssumeNotNull().OptionalOrDefault()
             )
         select new PythonFunctionParameter(arg.Name, type,
                                            // Force a default value for *args and **kwargs as null, otherwise the calling convention is strange
                                            arg.ParameterType is PythonFunctionParameterType.Star or PythonFunctionParameterType.DoubleStar
                                                && defaultValue is null
                                                ? PythonConstant.None.Value
                                                : defaultValue,
                                            arg.ParameterType))
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter?> ParameterOrSlash { get; } =
        PositionalOnlyParameterParser.AsNullable()
        .Or(PythonParameterTokenizer.AsNullable())
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter[]> PythonParameterListTokenizer { get; } =
        (from openParen in Token.EqualTo(PythonToken.OpenParenthesis)
         from parameters in ParameterOrSlash.ManyDelimitedBy(Token.EqualTo(PythonToken.Comma))
         from closeParen in Token.EqualTo(PythonToken.CloseParenthesis)
         select parameters)
        .Named("Parameter List");
}
