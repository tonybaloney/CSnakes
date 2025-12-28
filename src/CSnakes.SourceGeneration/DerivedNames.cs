namespace CSnakes.SourceGeneration;

public sealed record DerivedNames
{
    public required string Namespace { get; init; }
    public required string GeneratedFileName { get; init; }
    public required string PascalFileName { get; init; }
    public required string ModuleAbsoluteName { get; init; }
}
