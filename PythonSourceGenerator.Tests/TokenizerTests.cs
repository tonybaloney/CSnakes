
using PythonSourceGenerator.Parser;
using Superpower;

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

    [Theory]
    [InlineData("a: int", "a", "int")]
    [InlineData("abc123_xyz: int", "abc123_xyz", "int")]
    [InlineData("b: str", "b", "str")]
    [InlineData("c: float", "c", "float")]
    [InlineData("d: bool", "d", "bool")]
    // [InlineData("e: list[int]", "e", "list[int]")]
    // [InlineData("f: tuple[str, str]", "f", "tuple[str, str]")]
    // [InlineData("g: dict[str, int]", "g", "dict[str, int]")]
    public void ParseFunctionParameter(string code, string expectedName, string expectedType)
    {
        var tokens = PythonSignatureTokenizer.Tokenize(code);
        var result = PythonSignatureParser.PythonParameter.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedType, result.Value.Type);
    }
}
