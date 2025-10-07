namespace CSnakes.Reflection;

/// <summary>
/// Represents <see
/// href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters#safe-context-of-references-and-values">
/// safety context of references and values</see>.
/// </summary>
public enum RefSafetyContext
{
    /// <summary>
    /// Scope where any expression can be safely accessed.
    /// </summary>
    Safe,

    /// <summary>
    /// Scope where a <em>reference</em> to any expression can be safely accessed or modified.
    /// </summary>
    RefSafe,
}
