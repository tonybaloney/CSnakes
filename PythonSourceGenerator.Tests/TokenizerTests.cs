
using PythonSourceGenerator.Parser;

namespace PythonSourceGenerator.Tests;
public class TokenizerTests
{

    [Fact]
    public void Tokenize()
    {
        var tokens = PythonSignatureTokenizer.Tokenize("def foo(a: int, b: str) -> None:");
        Assert.Equal(new[]
        {
            PythonSignatureTokens.PythonSignatureToken.Def,
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.OpenParenthesis,
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.Colon,
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.Comma,
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.Colon,
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.CloseParenthesis,
            PythonSignatureTokens.PythonSignatureToken.Arrow,
            PythonSignatureTokens.PythonSignatureToken.None,
            PythonSignatureTokens.PythonSignatureToken.Colon,
        }, tokens.Select(t => t.Kind));
    }
}
