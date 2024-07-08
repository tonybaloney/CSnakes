using Microsoft.CodeAnalysis;
using PythonSourceGenerator.Parser;
using Superpower;

namespace PythonSourceGenerator.Tests;
public class TokenizerTests
{

    [Fact]
    public void Tokenize()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize("def foo(a: int, b: str) -> None:");
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

    [Theory]
    [InlineData("1", PythonSignatureTokens.PythonSignatureToken.Integer)]
    [InlineData("-1", PythonSignatureTokens.PythonSignatureToken.Integer)]
    [InlineData("10123", PythonSignatureTokens.PythonSignatureToken.Integer)]
    [InlineData("-1231", PythonSignatureTokens.PythonSignatureToken.Integer)]
    [InlineData("1.0", PythonSignatureTokens.PythonSignatureToken.Decimal)]
    [InlineData("-1123.323", PythonSignatureTokens.PythonSignatureToken.Decimal)]
    [InlineData("abc123", PythonSignatureTokens.PythonSignatureToken.Identifier)]
    [InlineData("'hello'", PythonSignatureTokens.PythonSignatureToken.SingleQuotedString)]
    [InlineData("\"hello\"", PythonSignatureTokens.PythonSignatureToken.DoubleQuotedString)]
    public void AssertTokenKinds(string code, PythonSignatureTokens.PythonSignatureToken expectedToken)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
        Assert.Equal(expectedToken, tokens.Single().Kind);
    }

    [Fact]
    public void TokenizeWithDefaultValue()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize("def foo(a: int, b: str = 'b') -> None:");
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
            PythonSignatureTokens.PythonSignatureToken.SingleQuotedString,
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
    [InlineData("nest: list[tuple[int, int]]", "nest", "list[tuple[int, int]]")]
    [InlineData("max_length: int", "max_length", "int")]
    [InlineData("max_length: int = 50", "max_length", "int")]
    public void ParseFunctionParameter(string code, string expectedName, string expectedType)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedType, result.Value.Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterNoType()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize("a");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefault()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize("a = 1");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("1", result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Theory]
    [InlineData("123")]
    [InlineData("-1")]
    [InlineData("1.01")]
    [InlineData("-1.01")]
    public void ParseFunctionParameterDefaultValuesNoType(string value)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize($"a = {value}");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal(value, result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultSingleQuotedString()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize($"a = 'hello'");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("hello", result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultDoubleQuotedString()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize($"a = \"hello\"");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("hello", result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultDouble()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize($"a: float = 0.0");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("0.0", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultInt()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize($"a: int = 1234");
        var result = PythonSignatureParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("1234", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterListSingleGeneric()
    {
        var code = "(a: list[int])";
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("list[int]", result.Value[0].Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListDoubleGeneric()
    {
        var code = "(a: list[int], b)";
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("list[int]", result.Value[0].Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListEasy()
    {
        var code = "(a: int, b: float, c: str)";
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
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
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
        var result = PythonSignatureParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("Any", result.Value[0].Type.Name);
        Assert.Equal("b", result.Value[1].Name);
        Assert.Equal("Any", result.Value[1].Type.Name);
        Assert.Equal("c", result.Value[2].Name);
        Assert.Equal("Any", result.Value[2].Type.Name);
    }

    [Fact]
    public void ParseFunctionDefinition()
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize("def foo(a: int, b: str) -> None:");
        var result = PythonSignatureParser.PythonFunctionDefinitionTokenizer.TryParse(tokens);

        Assert.True(result.HasValue);
        Assert.Equal("foo", result.Value.Name);
        Assert.Equal("a", result.Value.Parameters[0].Name);
        Assert.Equal("int", result.Value.Parameters[0].Type.Name);
        Assert.Equal("b", result.Value.Parameters[1].Name);
        Assert.Equal("str", result.Value.Parameters[1].Type.Name);
        Assert.Equal("None", result.Value.ReturnType.Name);
    }

    [Theory]
    // Walk up the complexity
    [InlineData("def foo(a: int, b: str) -> None:")]
    [InlineData("def foo(a: int, b: str = 'b') -> str:")]
    [InlineData("def foo(a: int, b: str = 'b', c: float = 1.0) -> float:")]
    [InlineData("def foo(a: int, c: list[int]) -> None:")]
    [InlineData("def foo(data: list[int], clusters: int) -> float:")]
    // From the demo apps
    [InlineData("def format_name(name: str, max_length: int = 50) -> str:")]
    [InlineData("def calculate_kmeans_interia(data: list[tuple[int, int]], n_clusters: int) -> float:")]
    [InlineData("def invoke_mistral_inference(messages: list[str], lang: str = \"en-US\", temperature=0.0) -> str:")]
    public void ParseFunctionDefinitionComplex(string code)
    {
        var tokens = PythonSignatureTokenizer.Instance.Tokenize(code);
        Assert.True(tokens.IsAtEnd == false, "Tokenize failed");
        var result = PythonSignatureParser.PythonFunctionDefinitionTokenizer.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
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
        _ = PythonSignatureParser.TryParseFunctionDefinitions(code, out var functions, out var errors);

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

    [Fact]
    public void ParseMultiLineFunctionDefinition()
    {
        var code = @"""
import foo

def bar(a: int, 
        b: str) -> None:
    pass

a = 1

if __name__ == '__main__':
  xyz  = 1
        """;
        _ = PythonSignatureParser.TryParseFunctionDefinitions(code, out var functions, out var errors);

        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("bar", functions[0].Name);
        Assert.Equal("a", functions[0].Parameters[0].Name);
        Assert.Equal("int", functions[0].Parameters[0].Type.Name);
        Assert.Equal("b", functions[0].Parameters[1].Name);
        Assert.Equal("str", functions[0].Parameters[1].Type.Name);
        Assert.Equal("None", functions[0].ReturnType.Name);
    }
}
