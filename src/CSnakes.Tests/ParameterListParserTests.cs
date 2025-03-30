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
    [InlineData("*, a = 1, b, c", "a, b, c")]
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
    [InlineData("a = 1, b, c", "Syntax error (line 1, column 1): non-default argument follows default argument.")]
    [InlineData("a, b = 1, c", "Syntax error (line 1, column 1): non-default argument follows default argument.")]
    [InlineData("a = 1, b, c, /", "Syntax error (line 1, column 1): non-default argument follows default argument.")]
    [InlineData("a = 1, /, b, c", "Syntax error (line 1, column 1): non-default argument follows default argument.")]
    [InlineData("a, /, b = 1, c", "Syntax error (line 1, column 1): non-default argument follows default argument.")]
    // The following test cases were taken directly from the CPython test suite:
    // https://github.com/python/cpython/blob/ffc2f1dd1c023b44b488311511db790a96d757db/Lib/test/test_syntax.py#L385-L537
    [InlineData("None=1", "Syntax error (line 1, column 2): unexpected none `None`, expected `)`.")]                // invalid syntax
    [InlineData("x, y=1, z", "Syntax error (line 1, column 1): non-default argument follows default argument.")]    // parameter without a default follows parameter with a default
    [InlineData("x, /, y=1, z", "Syntax error (line 1, column 1): non-default argument follows default argument.")] // parameter without a default follows parameter with a default
    [InlineData("x, None", "Syntax error (line 1, column 5): unexpected none `None`, expected Parameter.")]         // invalid syntax
    [InlineData("*None", "Syntax error (line 1, column 3): unexpected none `None`, expected Parameter or `,`.")]    // invalid syntax
    [InlineData("**None", "Syntax error (line 1, column 4): unexpected none `None`, expected Parameter.")]          // invalid syntax
    [InlineData("/,a,b=,c", "Syntax error (line 1, column 2): unexpected `/`, expected `)`.")]                      // at least one argument must precede /
    [InlineData("a,/,/,b,c", "Syntax error (line 1, column 5): unexpected `,/`, expected `)`.")]                    // / may appear only once
    [InlineData("a,/,a1,/,b,c", "Syntax error (line 1, column 8): unexpected `,/`, expected `)`.")]                 // / may appear only once
    [InlineData("a=1,/,/,*b,/,c", "Syntax error (line 1, column 7): unexpected `,/`, expected `)`.")]               // / may appear only once
    [InlineData("a,/,a1=1,/,b,c", "Syntax error (line 1, column 10): unexpected `,/`, expected `)`.")]              // / may appear only once
    [InlineData("a,*b,c,/,d,e", "Syntax error (line 1, column 8): unexpected `,/`, expected `)`.")]                 // / must be ahead of *
    [InlineData("a=1,*b,c=3,/,d,e", "Syntax error (line 1, column 12): unexpected `,/`, expected `)`.")]            // / must be ahead of *
    [InlineData("a,*b=3,c", "Syntax error (line 1, column 6): unexpected `=`, expected `)`.")]                      // var-positional argument cannot have default value
    [InlineData("a,*b: int=,c", "Syntax error (line 1, column 11): unexpected `=`, expected `)`.")]                 // var-positional argument cannot have default value
    [InlineData("a,**b=3", "Syntax error (line 1, column 7): unexpected `=`, expected `)`.")]                       // var-keyword argument cannot have default value
    [InlineData("a,**b: int=3", "Syntax error (line 1, column 12): unexpected `=`, expected `)`.")]                 // var-keyword argument cannot have default value
    [InlineData("a,*a, b, **c, d", "Syntax error (line 1, column 14): unexpected `,`, expected `)`.")]              // arguments cannot follow var-keyword argument
    [InlineData("a,*a, b, **c, d=4", "Syntax error (line 1, column 14): unexpected `,`, expected `)`.")]            // arguments cannot follow var-keyword argument
    [InlineData("a,*a, b, **c, *d", "Syntax error (line 1, column 14): unexpected `, *`, expected `)`.")]           // arguments cannot follow var-keyword argument
    [InlineData("a,*a, b, **c, **d", "Syntax error (line 1, column 14): unexpected `, **`, expected `)`.")]         // arguments cannot follow var-keyword argument
    [InlineData("a=1,/,**b,/,c", "Syntax error (line 1, column 11): unexpected `,/`, expected `)`.")]               // arguments cannot follow var-keyword argument
    [InlineData("*b,*d", "Syntax error (line 1, column 4): unexpected `,*`, expected `)`.")]                        // * argument may appear only once
    [InlineData("a,*b,c,*d,*e,c", "Syntax error (line 1, column 8): unexpected `,*`, expected `)`.")]               // * argument may appear only once
    [InlineData("a,b,/,c,*b,c,*d,*e,c", "Syntax error (line 1, column 14): unexpected `,*`, expected `)`.")]        // * argument may appear only once
    [InlineData("a,b,/,c,*b,c,*d,**e", "Syntax error (line 1, column 14): unexpected `,*`, expected `)`.")]         // * argument may appear only once
    [InlineData("a=1,/*,b,c", "Syntax error (line 1, column 7): unexpected `*`, expected `)`.")]                    // expected comma between / and *
    [InlineData("a=1,d=,c", "Syntax error (line 1, column 8): unexpected `,`, expected Constant.")]                 // expected default value expression
    [InlineData("a,d=,c", "Syntax error (line 1, column 6): unexpected `,`, expected Constant.")]                   // expected default value expression
    [InlineData("a,d: int=,c", "Syntax error (line 1, column 11): unexpected `,`, expected Constant.")]             // expected default value expression
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
