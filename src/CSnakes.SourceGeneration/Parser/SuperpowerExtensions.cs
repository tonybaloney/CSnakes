using Superpower;
using Superpower.Model;

namespace CSnakes.Parser;

static class SuperpowerExtensions
{
    /// <summary>
    /// Ignores the result of the second parser, returning the result of the first.
    /// </summary>
    public static TokenListParser<TKind, T> ThenIgnore<TKind, T, U>(this TokenListParser<TKind, T> first,
                                                                    TokenListParser<TKind, U> second) =>
        from a in first
        from _ in second
        select a;

    // TODO: Replace `CharSpan` with `TextSpan.AsReadOnlySpan` when available in Superpower
    // See: https://github.com/datalust/superpower/blob/998091233e5ef7624be349b8df32ee6c72588a1c/src/Superpower/Model/TextSpan.cs#L240-L244

    public static ReadOnlySpan<char> CharSpan(this Token<PythonToken> token) => token.Span.CharSpan();

    public static ReadOnlySpan<char> CharSpan(this TextSpan span) =>
        span.Source is { } source
            ? source.AsSpan(span.Position.Absolute, span.Length)
            : ReadOnlySpan<char>.Empty;
}
