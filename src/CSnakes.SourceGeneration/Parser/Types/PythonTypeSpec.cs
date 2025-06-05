using System.Collections.Immutable;

namespace CSnakes.Parser.Types;
public class PythonTypeSpec(string name, ImmutableArray<PythonTypeSpec> arguments)
{
    public string Name { get; } = name;

    public ImmutableArray<PythonTypeSpec> Arguments { get; } = arguments;

    public override string ToString() =>
        Arguments is { Length: > 0 } args ?
            $"{Name}[{string.Join(", ", args.Select(a => a.ToString()))}]" :
            Name;

    public static readonly PythonTypeSpec Any = new("Any", []);
}
