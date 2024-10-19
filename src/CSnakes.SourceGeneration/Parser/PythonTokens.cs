using Superpower.Display;

namespace CSnakes.Parser;

public enum PythonToken
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
    QualifiedIdentifier,

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
    HexidecimalInteger,
    BinaryInteger,
    DoubleQuotedString,
    SingleQuotedString,
    True,
    False,
    None,

    [Token(Example = "...")]
    Ellipsis,

    [Token(Example = ", /")]
    CommaSlash,

    [Token(Example = ", *")]
    CommaStar,

    [Token(Example = ", **")]
    CommaStarStar,

    [Token(Example = ", *,")]
    CommaStarComma,

    [Token(Example = ", )")]
    CommaCloseParenthesis,
}
