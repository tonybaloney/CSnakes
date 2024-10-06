using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using CSnakes.Parser;
using CSnakes.Parser.Types;
using Superpower;

namespace CSnakes.Tests;
public class TokenizerTests
{

    [Fact]
    public void Tokenize()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("def foo(a: int, b: str) -> None:");
        Assert.Equal(
        [
            PythonToken.Def,
            PythonToken.Identifier,
            PythonToken.OpenParenthesis,
            PythonToken.Identifier,
            PythonToken.Colon,
            PythonToken.Identifier,
            PythonToken.Comma,
            PythonToken.Identifier,
            PythonToken.Colon,
            PythonToken.Identifier,
            PythonToken.CloseParenthesis,
            PythonToken.Arrow,
            PythonToken.None,
            PythonToken.Colon,
        ], tokens.Select(t => t.Kind));
    }

    [Theory]
    [InlineData("1", PythonToken.Integer)]
    [InlineData("-1", PythonToken.Integer)]
    [InlineData("10123", PythonToken.Integer)]
    [InlineData("-1231", PythonToken.Integer)]
    [InlineData("1.0", PythonToken.Decimal)]
    [InlineData("-1123.323", PythonToken.Decimal)]
    [InlineData("abc123", PythonToken.Identifier)]
    [InlineData("'hello'", PythonToken.SingleQuotedString)]
    [InlineData("\"hello\"", PythonToken.DoubleQuotedString)]
    [InlineData("True", PythonToken.True)]
    [InlineData("False", PythonToken.False)]
    [InlineData("None", PythonToken.None)]
    public void AssertTokenKinds(string code, PythonToken expectedToken)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        Assert.Equal(expectedToken, tokens.Single().Kind);
    }

    [Fact]
    public void TokenizeWithDefaultValue()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("def foo(a: int, b: str = 'b') -> None:");
        Assert.Equal(
        [
            PythonToken.Def,
            PythonToken.Identifier,
            PythonToken.OpenParenthesis,
            PythonToken.Identifier,
            PythonToken.Colon,
            PythonToken.Identifier,
            PythonToken.Comma,
            PythonToken.Identifier,
            PythonToken.Colon,
            PythonToken.Identifier,
            PythonToken.Equal,
            PythonToken.SingleQuotedString,
            PythonToken.CloseParenthesis,
            PythonToken.Arrow,
            PythonToken.None,
            PythonToken.Colon,
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
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedType, result.Value.Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterNoType()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("a");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefault()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("a = 1");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
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
        var tokens = PythonTokenizer.Instance.Tokenize($"a = {value}");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal(value, result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultSingleQuotedString()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a = 'hello'");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("hello", result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultDoubleQuotedString()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a = \"hello\"");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("hello", result.Value.DefaultValue?.ToString());
        Assert.False(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultDouble()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: float = -1.1");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("-1.1", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultInt()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: int = 1234");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("1234", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultBoolTrue()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: bool = True");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("True", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultBoolFalse()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: bool = False");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("False", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterDefaultNone()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: bool = None");
        var result = PythonParser.PythonParameterTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value.Name);
        Assert.Equal("None", result.Value.DefaultValue?.ToString());
        Assert.True(result.Value.HasTypeAnnotation());
    }

    [Fact]
    public void ParseFunctionParameterListSingleGeneric()
    {
        var code = "(a: list[int])";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("list[int]", result.Value[0].Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListDoubleGeneric()
    {
        var code = "(a: list[int], b)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("list[int]", result.Value[0].Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListQualifiedGenericType()
    {
        var code = "(a: typing.List[int], b)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("typing.List[int]", result.Value[0].Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListQualifiedBasicType()
    {
        var code = "(a: np.ndarray, b)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListTokenizer.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal("a", result.Value[0].Name);
        Assert.Equal("np.ndarray", result.Value[0].Type.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListEasy()
    {
        var code = "(a: int, b: float, c: str)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListTokenizer.TryParse(tokens);
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
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListTokenizer.TryParse(tokens);
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
        var tokens = PythonTokenizer.Instance.Tokenize("def foo(a: int, b: str) -> None:");
        var result = PythonParser.PythonFunctionDefinitionTokenizer.TryParse(tokens);

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
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        Assert.True(tokens.IsAtEnd == false, "Tokenize failed");
        var result = PythonParser.PythonFunctionDefinitionTokenizer.TryParse(tokens);
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

        SourceText sourceText = SourceText.From(code);

        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
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
        var code = @"
import foo

def bar(a: int, 
        b: str) -> None:
    pass

a = 1

if __name__ == '__main__':
  xyz  = 1
        ";

        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("bar", functions[0].Name);
        Assert.Equal("a", functions[0].Parameters[0].Name);
        Assert.Equal("int", functions[0].Parameters[0].Type.Name);
        Assert.Equal("b", functions[0].Parameters[1].Name);
        Assert.Equal("str", functions[0].Parameters[1].Type.Name);
        Assert.Equal("None", functions[0].ReturnType.Name);
    }

    [Fact]
    public void ParseFunctionWithTrailingComment()
    {
        var code = @"def bar(a: int, b: str) -> None: # this is a comment
    pass";
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("bar", functions[0].Name);
    }

    [Fact]
    public void ParseFunctionTrailingSpaceAfterColon()
    {
        var code = @"def bar(a: int, 
        b: str) -> None:   
    pass"; // There is a trailing space after None:
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("bar", functions[0].Name);
    }

    [Fact]
    public void ParseFunctionNoBlankLineAtEnd()
    {
        var code = @"def bar(a: int, 
        b: str) -> None:
    pass";
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("bar", functions[0].Name);
        Assert.Equal("a", functions[0].Parameters[0].Name);
        Assert.Equal("int", functions[0].Parameters[0].Type.Name);
        Assert.Equal("b", functions[0].Parameters[1].Name);
        Assert.Equal("str", functions[0].Parameters[1].Type.Name);
        Assert.Equal("None", functions[0].ReturnType.Name);
    }

    [Fact]
    public void ErrorInParamSignatureReturnsCorrectLineAndColumn()
    {
        var code = """



        def bar(a: int, b:= str) -> None:
            pass
        """;
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var _, out var errors);
        Assert.NotEmpty(errors);
        Assert.Equal(3, errors[0].StartLine);
        Assert.Equal(5, errors[0].EndLine);
        Assert.Equal(21, errors[0].StartColumn);
        Assert.Equal(22, errors[0].EndColumn);
        Assert.Equal("unexpected identifier `str`, expected `)`", errors[0].Message);
    }

    [Fact]
    public void ErrorInParamDefaultValueReturnsCorrectLineAndColumn()
    {
        var code = """
        def test_octal_literal(a: int = 0o1234) -> None:
            pass
        """;
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var _, out var errors);
        Assert.NotEmpty(errors);
        Assert.Equal(0, errors[0].StartLine);
        Assert.Equal(0, errors[0].EndLine);
        Assert.Equal(33, errors[0].StartColumn);
        Assert.Equal(49, errors[0].EndColumn);
        Assert.Equal("invalid binaryinteger, unexpected `o`, expected `b` at line 1, column 34", errors[0].Message);
    }

    [Theory]
    /* See https://github.com/python/cpython/blob/main/Lib/test/test_tokenize.py#L2063-L2126 */
    [InlineData("255", 255)]
    [InlineData("0b10", 0b10)]
    [InlineData("0b10101101", 0b10101101)]
    [InlineData("0b1010_1101", 0b10101101)]
    // [InlineData("0o123", 0o123)] Octal literals are not supported in C#
    [InlineData("1234567", 1234567)]
    [InlineData("-1234567", -1234567)]
    [InlineData("0xdeadbeef", 0xdeadbeef)]
    [InlineData("0xdead_beef", 0xdeadbeef)]
    // See https://github.com/python/cpython/blob/main/Lib/test/test_grammar.py#L25
    [InlineData("1_000_000", 1_000_000)]
    [InlineData("4_2", 42)]
    [InlineData("0_0", 0)]
    public void TestIntegerTokenization(string code, long expectedValue)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        Assert.Single(tokens);
        var result = PythonParser.ConstantValueTokenizer.TryParse(tokens);

        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        var value = result.Value as PythonConstant.Integer;
        Assert.NotNull(value);
        Assert.Equal(expectedValue, value.Value);
    }

    [Theory]
    /* See https://github.com/python/cpython/blob/main/Lib/test/test_tokenize.py#L2063-L2126 */
    [InlineData("1.0", 1.0)]
    [InlineData("-1.0", -1.0)]
    [InlineData("-1_000.0", -1000.0)]
    [InlineData("3.14159", 3.14159)]
    [InlineData("314159.", 314159.0)]
    // [InlineData(".314159", .314159)] // TODO: (track) Support no leading 0
    // [InlineData(".1_4", 0.14)]
    // [InlineData(".1_4e1", 0.14e1)]
    [InlineData("3E123", 3E123)]
    [InlineData("3e-1230", 3e-1230)]
    [InlineData("3.14e159", 3.14e159)]
    [InlineData("1_000_000.4e5", 1_000_000.4e5)]
    [InlineData("1e1_0", 1e1_0)]
    public void TestDoubleTokenization(string code, double expectedValue)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        Assert.Single(tokens);
        Assert.Equal(PythonToken.Decimal, tokens.First().Kind);
        var result = PythonParser.DecimalConstantTokenizer.TryParse(tokens);

        Assert.True(result.HasValue);
        var value = result.Value as PythonConstant.Float;
        Assert.NotNull(value);
        Assert.Equal(expectedValue, value.Value);
    }
}

