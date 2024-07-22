using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace PythonSourceGenerator.Parser;
public static partial class PythonSignatureParser
{
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonTypeSpec> PythonTypeDefinitionTokenizer { get; } =
        (from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier).Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.None))
         from openBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenBracket)
            .Then(_ => PythonTypeDefinitionTokenizer.ManyDelimitedBy(
                Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma),
                end: Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseBracket)
                ))
            .OptionalOrDefault()
         select new PythonTypeSpec(name.ToStringValue(), openBracket))
        .Named("Type Definition");
}
