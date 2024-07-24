using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace PythonSourceGenerator.Parser;
public static partial class PythonSignatureParser
{
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter> PythonParameterTokenizer { get; } =
        (from arg in PythonArgTokenizer
         from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon).Optional()
         from type in PythonTypeDefinitionTokenizer.OptionalOrDefault()
         from defaultValue in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Equal).Optional().Then(
                 _ => ConstantValueTokenizer.OptionalOrDefault()
             )
         select new PythonFunctionParameter(arg.Name, type, defaultValue, arg.ParameterType))
        .Named("Parameter");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter[]> PythonParameterListTokenizer { get; } =
        (from openParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenParenthesis)
         from parameters in PythonParameterTokenizer.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))
         from closeParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseParenthesis)
         select parameters)
        .Named("Parameter List");
}
