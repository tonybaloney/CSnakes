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

    public static TokenListParser<PythonToken, PythonFunctionParameter> PythonParameterParser { get; } =
        (from arg in PythonArgParser
         from type in Token.EqualTo(PythonToken.Colon).Optional().Then(
                _ => PythonTypeDefinitionParser.AssumeNotNull().OptionalOrDefault()
             )
         from defaultValue in Token.EqualTo(PythonToken.Equal).Optional().Then(
                 _ => ConstantValueTokenizer.AssumeNotNull().OptionalOrDefault()
             )
         select new PythonFunctionParameter(arg.Name, type, defaultValue, arg.ParameterType))
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter?> ParameterOrSlashParser { get; } =
        PositionalOnlyParameterParser.AsNullable()
        .Or(PythonParameterParser.AsNullable())
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter[]> PythonParameterListParser { get; } =
        (from openParen in Token.EqualTo(PythonToken.OpenParenthesis)
         from parameters in ParameterOrSlashParser.OptionalOrDefault().ManyDelimitedBy(Token.EqualTo(PythonToken.Comma), Token.EqualTo(PythonToken.CloseParenthesis))
         select parameters.Where(p => p is not null).ToArray()
         )
        .Named("Parameter List");
}
