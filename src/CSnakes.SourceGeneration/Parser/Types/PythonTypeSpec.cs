using System.Collections.Immutable;

namespace CSnakes.Parser.Types;
public sealed class PythonTypeSpec(string name, ImmutableArray<PythonTypeSpec> arguments = default)
{
    public string Name { get; } = name;

    public ImmutableArray<PythonTypeSpec> Arguments { get; } = !arguments.IsDefault ? arguments : [];

    public override string ToString() =>
        Arguments is { Length: > 0 } args ?
            $"{Name}[{string.Join(", ", args.Select(a => a.ToString()))}]" :
            Name;

    public static readonly PythonTypeSpec Any = new("Any");
    public static readonly PythonTypeSpec None = new("None");

    public static PythonTypeSpec Optional(PythonTypeSpec type) => new("Optional", [type]);
    public static PythonTypeSpec Literal(ImmutableArray<PythonConstant> values) =>
        new("Literal") /* TODO: Capture literal values */;
}
