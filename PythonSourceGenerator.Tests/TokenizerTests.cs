using PythonSourceGenerator.Parser;
using Superpower;

namespace PythonSourceGenerator.Tests;
public class TokenizerTests
{

    [Fact]
    public void Tokenize()
    {
        var tokens = PythonSignatureTokenizer.Tokenize("def foo(a: int, b: str) -> None:");
        Assert.Equal(
        [
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
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.Colon,
        ], tokens.Select(t => t.Kind));
    }

    [Fact]
    public void TokenizeWithDefaultValue()
    {
        var tokens = PythonSignatureTokenizer.Tokenize("def foo(a: int, b: str = 'b') -> None:");
        Assert.Equal(
        [
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
            PythonSignatureTokens.PythonSignatureToken.Equal,
            PythonSignatureTokens.PythonSignatureToken.CloseParenthesis,
            PythonSignatureTokens.PythonSignatureToken.Arrow,
            PythonSignatureTokens.PythonSignatureToken.Identifier,
            PythonSignatureTokens.PythonSignatureToken.Colon,
        ], tokens.Select(t => t.Kind));
    }

    [Theory]
    [InlineData("a: int", "a", "int")]
    [InlineData("abc123_xyz: int", "abc123_xyz", "int")]
    [InlineData("b: str", "b", "str")]
    [InlineData("c: float", "c", "float")]
    [InlineData("d: bool", "d", "bool")]
    [InlineData("e: list[int]", "e", "list[int]")]
    [InlineData("f: tuple[str, str]", "f", "tuple[str, str]")]
    [InlineData("g: dict[str, int]", "g", "dict[str, int]")]
    public void ParseFunctionParameter(string code, string expectedName, string expectedType)
    {
        var tokens = PythonSignatureTokenizer.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedType, result.Value.Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterNoType()
    {
        var tokens = PythonSignatureTokenizer.Tokenize("a");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Null(result.Value.Type);
    }

    [Fact]
    public void ParseFunctionParameterListEasy()
    {
        var code = "(a: int, b: float, c: str)";
        var tokens = PythonSignatureTokenizer.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("int", result.Value[0].Type.Name);
        Assert.Equal("b", result.Value[1].Name);
        Assert.Equal("float", result.Value[1].Type.Name);
        Assert.Equal("c", result.Value[2].Name);
        Assert.Equal("str", result.Value[2].Type.Name);
    }

    [Fact]
    public void ParseFunctionParameterListUntyped()
    {
        var code = "(a, b, c)";
        var tokens = PythonSignatureTokenizer.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Null(result.Value[0].Type);
        Assert.Equal("b", result.Value[1].Name);
        Assert.Null(result.Value[1].Type);
        Assert.Equal("c", result.Value[2].Name);
        Assert.Null(result.Value[2].Type);
    }

    [Fact]
    public void ParseFunctionDefinition()
    {
        var tokens = PythonSignatureTokenizer.Tokenize("def foo(a: int, b: str) -> None:");
        var result = PythonSignatureParser.PythonFunctionDefinitionTokenizer.TryParse(tokens);

        Assert.True(result.HasValue);
        Assert.Equal("foo", result.Value.Name);
        Assert.Equal("a", result.Value.Parameters[0].Name);
        Assert.Equal("int", result.Value.Parameters[0].Type.Name);
        Assert.Equal("b", result.Value.Parameters[1].Name);
        Assert.Equal("str", result.Value.Parameters[1].Type.Name);
        Assert.Equal("None", result.Value.ReturnType.Name);
    }

    [Fact]
    public void ParseFullCode()
    {
        var code = @"""
import foo

def bar(a: int, b: str) -> None:
    pass

def baz(c: float, d: bool) -> None:
    ...

a = 1

if __name__ == '__main__':
  xyz  = 1
        """;
        _ = PythonSignatureParser.TryParseFunctionDefinitions(code, out var functions);

        Assert.NotNull(functions);
        Assert.Equal(2, functions.Length);
        Assert.Equal("bar", functions[0].Name);
        Assert.Equal("a", functions[0].Parameters[0].Name);
        Assert.Equal("int", functions[0].Parameters[0].Type.Name);
        Assert.Equal("b", functions[0].Parameters[1].Name);
        Assert.Equal("str", functions[0].Parameters[1].Type.Name);
        Assert.Equal("None", functions[0].ReturnType.Name);

        Assert.Equal("baz", functions[1].Name);
        Assert.Equal("c", functions[1].Parameters[0].Name);
        Assert.Equal("float", functions[1].Parameters[0].Type.Name);
        Assert.Equal("d", functions[1].Parameters[1].Name);
        Assert.Equal("bool", functions[1].Parameters[1].Type.Name);
        Assert.Equal("None", functions[1].ReturnType.Name);
    }
}
