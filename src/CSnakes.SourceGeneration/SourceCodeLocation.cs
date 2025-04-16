using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSnakes.SourceGeneration;

public readonly record struct SourceCodeLocation(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static SourceCodeLocation? CreateFrom(Location location) =>
        location is { SourceTree.FilePath: var filePath } someLocation
            ? new SourceCodeLocation(filePath, someLocation.SourceSpan, someLocation.GetLineSpan().Span)
            : null;
}
