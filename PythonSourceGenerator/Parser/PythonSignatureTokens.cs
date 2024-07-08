using Superpower.Display;

namespace PythonSourceGenerator.Parser;
public class PythonSignatureTokens
{
    public enum PythonSignatureToken
    {
        [Token(Example = "(")]
        OpenParenthesis,

        [Token(Example = ")")]
        CloseParenthesis,

        [Token(Example = "[")]
        OpenBracket,

        [Token(Example = "]")]
        CloseBracket,

        [Token(Example = ":")]
        Colon,

        [Token(Example = ",")]
        Comma,

        [Token(Example = "*")]
        Asterisk,

        [Token(Example = "**")]
        DoubleAsterisk,

        Identifier,

        [Token(Example = "->")]
        Arrow,

        [Token(Example = "/")]
        Slash,

        [Token(Example = "=")]
        Equal,

        [Token(Example = "def")]
        Def,

        [Token(Example = "async")]
        Async,

        Integer,
        Decimal,
        DoubleQuotedString,
        SingleQuotedString,
        True,
        False,
        None,

        [Token(Example = "...")]
        Ellipsis
    }
}
