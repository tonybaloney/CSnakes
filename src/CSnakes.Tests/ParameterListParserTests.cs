using CSnakes.Parser;
using CSnakes.Parser.Types;
using Superpower;

namespace CSnakes.Tests;
public class ParameterListParserTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("a, b, c", "a, b, c")]
    [InlineData("a, b, c, /", "a, b, c")]
    [InlineData("a, b, c, /, d, e, f", "a, b, c, d, e, f")]
    [InlineData("*, a", "a")]
    [InlineData("a, b, /, c, d, *, e, f", "a, b, c, d, e, f")]
    [InlineData("a, b, /, *, c, d", "a, b, c, d")]
    [InlineData("*args", "args")]
    [InlineData("*args, a", "args, a")]
    [InlineData("a, *args", "a, args")]
    [InlineData("*args, a, b, c", "args, a, b, c")]
    [InlineData("a, b, c, *args", "a, b, c, args")]
    [InlineData("a, /, *args", "a, args")]
    [InlineData("a, /, b, *args", "a, b, args")]
    [InlineData("**kwargs", "kwargs")]
    [InlineData("a, **kwargs", "a, kwargs")]
    [InlineData("a, b, c, **kwargs", "a, b, c, kwargs")]
    [InlineData("a, b, c, /, d, e, f, *, g, h, i, **kwargs", "a, b, c, d, e, f, g, h, i, kwargs")]
    [InlineData("a, b, c, /, d, e, f, *args, g, h, i, **kwargs", "a, b, c, d, e, f, args, g, h, i, kwargs")]
    [InlineData("a, /, *, b, **kwargs", "a, b, kwargs")]
    [InlineData("*args, **kwargs", "args, kwargs")]
    [InlineData("*args, a, b, **kwargs", "args, a, b, kwargs")]
    public void Valid(string input, string expected)
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"({input})");
        var result = PythonParser.PythonParameterListParser.Parse(tokens);
        Assert.Equal(expected, string.Join(", ", from p in result.Enumerable() select p.Name));
    }

    [Theory]
    [InlineData("/", "Syntax error (line 1, column 2): unexpected `/`, expected `)`.")]
    [InlineData("*", "Syntax error (line 1, column 3): unexpected `)`, expected Parameter or `,`.")]
    [InlineData("*, /", "Syntax error (line 1, column 3): unexpected `, /`, expected Parameter or `,`.")]
    [InlineData("/, *", "Syntax error (line 1, column 2): unexpected `/`, expected `)`.")]
    [InlineData("a, /, /", "Syntax error (line 1, column 6): unexpected `, /`, expected `)`.")]
    [InlineData("a, *", "Syntax error (line 1, column 6): unexpected `)`, expected Parameter or `,`.")]
    [InlineData("*, *", "Syntax error (line 1, column 3): unexpected `, *`, expected Parameter or `,`.")]
    [InlineData("*args, *", "Syntax error (line 1, column 7): unexpected `, *`, expected `)`.")]
    [InlineData("*, *args", "Syntax error (line 1, column 3): unexpected `, *`, expected Parameter or `,`.")]
    [InlineData("*args, /", "Syntax error (line 1, column 7): unexpected `, /`, expected `)`.")]
    [InlineData("a, *args, /", "Syntax error (line 1, column 10): unexpected `, /`, expected `)`.")]
    [InlineData("*args, a, b, c, /", "Syntax error (line 1, column 16): unexpected `, /`, expected `)`.")]
    [InlineData("**kwargs, a", "Syntax error (line 1, column 10): unexpected `,`, expected `)`.")]
    [InlineData("**a, **b", "Syntax error (line 1, column 5): unexpected `, **`, expected `)`.")]
    [InlineData("**", "Syntax error (line 1, column 4): unexpected `)`, expected Parameter.")]
    [InlineData("a, *, b, **c, **d", "Syntax error (line 1, column 14): unexpected `, **`, expected `)`.")]
    [InlineData("a, /, *, b, **c, **d", "Syntax error (line 1, column 17): unexpected `, **`, expected `)`.")]
    [InlineData("*args = 0", "Syntax error (line 1, column 8): unexpected `=`, expected `)`.")]
    [InlineData("a, *args = 0", "Syntax error (line 1, column 11): unexpected `=`, expected `)`.")]
    [InlineData("**kwargs = 0", "Syntax error (line 1, column 11): unexpected `=`, expected `)`.")]
    [InlineData("a, **kwargs = 0", "Syntax error (line 1, column 14): unexpected `=`, expected `)`.")]
    [InlineData("foo, bar.baz, qux", "Syntax error (line 1, column 7): unexpected qualified identifier `bar.baz`, expected Parameter.")]
    public void Invalid(string input, string expected)
    {
        var tokens = PythonTokenizer.Instance.Tokenize($"({input})");
        void Act() => PythonParser.PythonParameterListParser.Parse(tokens);
        var ex = Assert.Throws<ParseException>(Act);
        Assert.Equal(expected, ex.Message);
    }

    [Theory]
    [InlineData("(", "Syntax error: unexpected end of input, expected `)`.")]
    [InlineData("(a", "Syntax error: unexpected end of input, expected `)`.")]
    [InlineData("(a,", "Syntax error: unexpected end of input, expected Parameter.")]
    [InlineData("(a, b", "Syntax error: unexpected end of input, expected `)`.")]
    public void Unterminated(string input, string expected)
    {
        var tokens = PythonTokenizer.Instance.Tokenize(input);
        void Act() => PythonParser.PythonParameterListParser.Parse(tokens);
        var ex = Assert.Throws<ParseException>(Act);
        Assert.Equal(expected, ex.Message);
    }

}
