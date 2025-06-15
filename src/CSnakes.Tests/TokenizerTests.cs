using CSnakes.Parser;
using CSnakes.Parser.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Superpower;
using Superpower.Model;
using System.Runtime.CompilerServices;

namespace CSnakes.Tests;
public class TokenizerTests
{

    [Fact]
    public void Tokenize()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("def foo(a: int, b: str, c: typing.Optional[int], d: int | None) -> None:");
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
            PythonToken.Comma,
            PythonToken.Identifier,
            PythonToken.Colon,
            PythonToken.QualifiedIdentifier,
            PythonToken.OpenBracket,
            PythonToken.Identifier,
            PythonToken.CloseBracket,
            PythonToken.Comma,
            PythonToken.Identifier,
            PythonToken.Colon,
            PythonToken.Identifier,
            PythonToken.Pipe,
            PythonToken.None,
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
    [InlineData("foo.bar", PythonToken.QualifiedIdentifier)]
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
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal(expectedName, param.Name);
        Assert.Equal(expectedType, param.TypeSpec?.ToString());
    }

    [Fact]
    public void ParseFunctionParameterNoType()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("a");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        Assert.Same(PythonTypeSpec.Any, param.ImpliedTypeSpec);
        Assert.Null(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefault()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("a = 1");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.Integer>(param.DefaultValue);
        Assert.Equal(1, constant.Value);
        Assert.Same(PythonTypeSpec.Any, param.ImpliedTypeSpec);
        Assert.Null(param.TypeSpec);
    }

    [Theory]
    [InlineData(123L)]
    [InlineData(-1L)]
    [InlineData(1.01)]
    [InlineData(-1.01)]
    public void ParseFunctionParameterDefaultValuesNoType(object value)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(FormattableString.Invariant($"a = {value}"));
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        Assert.Equal(value, param.DefaultValue switch
        {
            PythonConstant.Float { Value: var n } => (object)n,
            PythonConstant.Integer { Value: var n } => n,
            var other => throw new SwitchExpressionException(other)
        });
        Assert.Same(PythonTypeSpec.Any, param.ImpliedTypeSpec);
        Assert.Null(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultSingleQuotedString()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a = 'hello'");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        var constant = Assert.IsType<PythonConstant.String>(param.DefaultValue);
        Assert.Equal("hello", constant.Value);
        Assert.Same(PythonTypeSpec.Any, param.ImpliedTypeSpec);
        Assert.Null(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultDoubleQuotedString()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a = \"hello\"");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.String>(param.DefaultValue);
        Assert.Equal("hello", constant.Value);
        Assert.Same(PythonTypeSpec.Any, param.ImpliedTypeSpec);
        Assert.Null(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultDouble()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: float = -1.1");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.Float>(param.DefaultValue);
        Assert.Equal(-1.1, constant.Value);
        Assert.NotNull(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultInt()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: int = 1234");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.Integer>(param.DefaultValue);
        Assert.Equal(1234, constant.Value);
        Assert.NotNull(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultBool()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: bool = True");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.Bool>(param.DefaultValue);
        Assert.True(constant.Value);
        Assert.NotNull(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultBoolFalse()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: bool = False");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.Bool>(param.DefaultValue);
        Assert.False(constant.Value);
        Assert.NotNull(param.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterDefaultNone()
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"a: bool = None");
        var result = PythonParser.OptionalPythonParameterParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var param = result.Value;
        Assert.Equal("a", param.Name);
        var constant = Assert.IsType<PythonConstant.None>(param.DefaultValue);
        Assert.Same(PythonConstant.None.Value, constant);
        Assert.NotNull(param.TypeSpec);
    }

    [Theory]
    [InlineData("args")]
    [InlineData("xs")]
    public void ParseFunctionVariadicPositionalParameter(string name)
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"(*{name})");
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(1, result.Value.Count);
        var parameter = result.Value.VariadicPositional;
        Assert.NotNull(parameter);
        Assert.Equal(name, parameter.Name);
        Assert.Same(PythonTypeSpec.Any, parameter.ImpliedTypeSpec);
        Assert.Null(parameter.TypeSpec);
    }

    [Theory]
    [InlineData("args")]
    [InlineData("xs")]
    public void ParseFunctionVariadicKeywordParameter(string name)
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"(**{name})");
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(1, result.Value.Count);
        var parameter = result.Value.VariadicKeyword;
        Assert.NotNull(parameter);
        Assert.Equal(name, parameter.Name);
        Assert.Same(PythonTypeSpec.Any, parameter.ImpliedTypeSpec);
        Assert.Null(parameter.TypeSpec);
    }

    [Theory]
    [InlineData("*args", 1)]
    [InlineData("a, *args", 2)]
    public void ParseFunctionSpecialStarParameter(string source, int expectedCount)
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"({source})");
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(expectedCount, result.Value.Count);
        var parameter = result.Value.VariadicPositional;
        Assert.NotNull(parameter);
        Assert.Equal("args", parameter.Name);
        Assert.Null(parameter.DefaultValue);
        Assert.Null(parameter.TypeSpec);
    }

    [Theory]
    [InlineData("**kwargs", 1)]
    [InlineData("a, **kwargs", 2)]
    public void ParseFunctionSpecialStarStarParameter(string source, int expectedCount)
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"({source})");
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        Assert.Equal(expectedCount, result.Value.Count);
        var parameter = result.Value.VariadicKeyword;
        Assert.NotNull(parameter);
        Assert.Equal("kwargs", parameter.Name);
        Assert.Null(parameter.DefaultValue);
        Assert.Null(parameter.TypeSpec);
    }

    [Fact]
    public void ParseFunctionParameterListSingleGeneric()
    {
        var code = "(a: list[int])";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var a = Assert.Single(result.Value.Enumerable());
        Assert.Equal("a", a.Name);
        Assert.Equal("list[int]", a.ImpliedTypeSpec.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListDoubleGeneric()
    {
        var code = "(a: list[int], b)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var a = result.Value.Enumerable().First();
        Assert.Equal("a", a.Name);
        Assert.Equal("list[int]", a.ImpliedTypeSpec.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListQualifiedGenericType()
    {
        var code = "(a: typing.List[int], b)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var a = result.Value.Enumerable().First();
        Assert.Equal("a", a.Name);
        Assert.Equal("typing.List[int]", a.ImpliedTypeSpec.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListQualifiedBasicType()
    {
        var code = "(a: np.ndarray, b)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var a = result.Value.Enumerable().First();
        Assert.Equal("a", a.Name);
        Assert.Equal("np.ndarray", a.ImpliedTypeSpec.ToString());
    }

    [Fact]
    public void ParseFunctionParameterListEasy()
    {
        var code = "(a: int, b: float, c: str)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var parameters = result.Value.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("int", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("float", b.ImpliedTypeSpec.Name);
        var c = parameters[2];
        Assert.Equal("c", c.Name);
        Assert.Equal("str", c.ImpliedTypeSpec.Name);
    }

    [Fact]
    public void ParseFunctionParameterListUntyped()
    {
        var code = "(a, b, c)";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var parameters = result.Value.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("Any", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("Any", b.ImpliedTypeSpec.Name);
        var c = parameters[2];
        Assert.Equal("c", c.Name);
        Assert.Equal("Any", c.ImpliedTypeSpec.Name);
    }

    [Fact]
    public void ParseFunctionParameterListTrailingComma()
    {
        var code = "(a, b, c, )";
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonParameterListParser.TryParse(tokens);
        Assert.True(result.HasValue);
        var parameters = result.Value.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("Any", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("Any", b.ImpliedTypeSpec.Name);
        var c = parameters[2];
        Assert.Equal("c", c.Name);
        Assert.Equal("Any", c.ImpliedTypeSpec.Name);
    }

    [Fact]
    public void ParseFunctionDefinition()
    {
        var tokens = PythonTokenizer.Instance.Tokenize("def foo(a: int, b: str) -> None:");
        var result = PythonParser.PythonFunctionDefinitionParser.TryParse(tokens);

        Assert.True(result.HasValue);
        Assert.Equal("foo", result.Value.Name);
        var parameters = result.Value.Parameters.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("int", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("str", b.ImpliedTypeSpec.Name);
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
        var result = PythonParser.PythonFunctionDefinitionParser.TryParse(tokens);
        Assert.True(result.HasValue, result.ToString());
    }

    [Theory]
    [InlineData("def foo() None:", "Syntax error (line 1, column 11): unexpected none `None`, expected `:`.")]
    public void InvalidFunctionDefinition(string code, string expectedError)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(code);
        var result = PythonParser.PythonFunctionDefinitionParser.TryParse(tokens);
        Assert.False(result.HasValue);
        Assert.Equal(expectedError, result.ToString());
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
        var parameters = functions[0].Parameters.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("int", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("str", b.ImpliedTypeSpec.Name);
        Assert.Equal("None", functions[0].ReturnType.Name);

        Assert.Equal("baz", functions[1].Name);
        parameters = functions[1].Parameters.Enumerable().ToArray();
        var c = parameters[0];
        Assert.Equal("c", c.Name);
        Assert.Equal("float", c.ImpliedTypeSpec.Name);
        var d = parameters[1];
        Assert.Equal("d", d.Name);
        Assert.Equal("bool", d.ImpliedTypeSpec.Name);
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
        var function = Assert.Single(functions);
        Assert.Equal("bar", function.Name);
        var parameters = function.Parameters.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("int", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("str", b.ImpliedTypeSpec.Name);
        Assert.Equal("None", function.ReturnType.Name);
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
    public void ParseFunctionMultiLineTrailingCommaWithComments()
    {
        const string code = """
        def a(    # this is a comment
            opener: str = 'foo', # type: ignore
        ) -> Any:
            pass
        """;
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("a", functions[0].Name);
        var parameters = functions[0].Parameters.Enumerable().ToArray();
        var opener = parameters[0];
        Assert.Equal("opener", opener.Name);
        Assert.Equal("str", opener.ImpliedTypeSpec.Name);
        Assert.NotNull(opener.DefaultValue);
        Assert.Equal("foo", opener.DefaultValue.ToString());
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
        var function = Assert.Single(functions);
        Assert.Equal("bar", function.Name);
        var parameters = function.Parameters.Enumerable().ToArray();
        var a = parameters[0];
        Assert.Equal("a", a.Name);
        Assert.Equal("int", a.ImpliedTypeSpec.Name);
        var b = parameters[1];
        Assert.Equal("b", b.Name);
        Assert.Equal("str", b.ImpliedTypeSpec.Name);
        Assert.Equal("None", function.ReturnType.Name);
    }

    [Fact]
    public void ParseFunctionMultiLineTrailingComma()
    {
        // This is common in Black-formatted code and has come up as a parser issue
        const string code = """
        def a(    
            opener: str = 'foo',
        ) -> Any:
            pass
        """;
        SourceText sourceText = SourceText.From(code);
        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        Assert.NotNull(functions);
        Assert.Single(functions);
        Assert.Equal("a", functions[0].Name);
        var parameters = functions[0].Parameters.Enumerable().ToArray();
        var opener = parameters[0];
        Assert.Equal("opener", opener.Name);
        Assert.Equal("str", opener.ImpliedTypeSpec.Name);
        Assert.NotNull(opener.DefaultValue);
        Assert.Equal("foo", opener.DefaultValue.ToString());
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
        Assert.Equal(3, errors[0].EndLine);
        Assert.Equal(19, errors[0].StartColumn);
        Assert.Equal(20, errors[0].EndColumn);
        Assert.Equal("unexpected `=`, expected Type Definition", errors[0].Message);
    }

    [Theory]
    /* See https://github.com/python/cpython/blob/main/Lib/test/test_tokenize.py#L2063-L2126 */
    [InlineData("255", 255)]
    [InlineData("0b10", 0b10)]
    [InlineData("0b10101101", 0b10101101)]
    [InlineData("0b1010_1101", 0b10101101)]
    [InlineData("0o1234", 0x29C)]
    [InlineData("0o777", 0x1FF)]
    [InlineData("0o0", 0x0)]
    [InlineData("0o1234567", 0x53977)]
    [InlineData("0o123_456_7", 0x53977)]
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
