using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonFunctionParameter> PythonParameterTokenizer { get; } =
        (from arg in PythonArgTokenizer
         from colon in Token.EqualTo(PythonToken.Colon).Optional()
         from type in PythonTypeDefinitionTokenizer.AssumeNotNull().OptionalOrDefault()
         from defaultValue in Token.EqualTo(PythonToken.Equal).Optional().Then(
                 _ => ConstantValueTokenizer.OptionalOrDefault()
             )
         select new PythonFunctionParameter(arg.Name, type, defaultValue, arg.ParameterType))
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter[]> PythonParameterListTokenizer { get; } =
        (from openParen in Token.EqualTo(PythonToken.OpenParenthesis)
         from parameters in PythonParameterTokenizer.ManyDelimitedBy(Token.EqualTo(PythonToken.Comma))
         from closeParen in Token.EqualTo(PythonToken.CloseParenthesis)
         select parameters)
        .Named("Parameter List");
}
