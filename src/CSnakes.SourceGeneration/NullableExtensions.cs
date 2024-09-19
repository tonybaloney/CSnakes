// credit: https://github.com/dotnet/razor/blob/main/src/Shared/Microsoft.AspNetCore.Razor.Utilities.Shared/NullableExtensions.cs

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace CSnakes;
internal static class NullableExtensions
{
    [DebuggerStepThrough]
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    public static T AssumeNotNull<T>(
        [NotNull] this T? obj,
        [CallerArgumentExpression(nameof(obj))] string? objExpression = null)
        where T : class
        => obj ?? throw new InvalidOperationException($"Expected '{objExpression}' to be non-null.");

    [DebuggerStepThrough]
    [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    public static T AssumeNotNull<T>(
        [NotNull] this T? obj,
        [CallerArgumentExpression(nameof(obj))] string? objExpression = null)
        where T : struct
        => obj ?? throw new InvalidOperationException($"Expected '{objExpression}' to be non-null.");
}