using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace PythonSourceGenerator.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonTypeSpec?> PythonTypeDefinitionTokenizer { get; } =
        (from name in Token.EqualTo(PythonToken.Identifier).Or(Token.EqualTo(PythonToken.None))
         from openBracket in Token.EqualTo(PythonToken.OpenBracket)
            .Then(_ => PythonTypeDefinitionTokenizer.ManyDelimitedBy(
                Token.EqualTo(PythonToken.Comma),
                Token.EqualTo(PythonToken.CloseBracket)))
            .OptionalOrDefault()
         select new PythonTypeSpec(name.ToStringValue(), openBracket))
        .Named("Type Definition");
}
