using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonFunctionParameter> PositinalOnlyParameterParser { get; } =
        Token.EqualTo(PythonToken.Slash)
        .Select(d => new PythonFunctionParameter("/", null, null, PythonFunctionParameterType.Slash))
        .Named("Positional Only Signal");

    public static TokenListParser<PythonToken, PythonFunctionParameter> PythonParameterTokenizer { get; } =
        (from arg in PythonArgTokenizer
         from colon in Token.EqualTo(PythonToken.Colon).Optional()
         from type in PythonTypeDefinitionTokenizer.AssumeNotNull().OptionalOrDefault()
         from defaultValue in Token.EqualTo(PythonToken.Equal).Optional().Then(
                 _ => ConstantValueTokenizer.OptionalOrDefault()
             )
         select new PythonFunctionParameter(arg.Name, type, defaultValue, arg.ParameterType))
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter?> ParameterOrSlash { get; } =
        PositinalOnlyParameterParser.AsNullable()
        .Or(PythonParameterTokenizer.AsNullable())
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter[]> PythonParameterListTokenizer { get; } =
        (from openParen in Token.EqualTo(PythonToken.OpenParenthesis)
         from parameters in ParameterOrSlash.ManyDelimitedBy(Token.EqualTo(PythonToken.Comma))
         from closeParen in Token.EqualTo(PythonToken.CloseParenthesis)
         select parameters)
        .Named("Parameter List");
}
