using Superpower;

namespace CSnakes.Parser;
static class ParserExtensions
{
    /// <summary>
    /// Ignores the result of the second parser, returning the result of the first.
    /// </summary>
    public static TokenListParser<TKind, T> ThenIgnore<TKind, T, U>(this TokenListParser<TKind, T> first,
                                                                    TokenListParser<TKind, U> second) =>
        from a in first
        from _ in second
        select a;
}
