using System.Collections.Immutable;

namespace CSnakes.Parser.Types;
public class PythonTypeSpec(string name, ImmutableArray<PythonTypeSpec> arguments)
{
    public string Name { get; } = name;

    public ImmutableArray<PythonTypeSpec> Arguments { get; } = arguments;

    public override string ToString() =>
        HasArguments() ?
            $"{Name}[{string.Join(", ", Arguments.Select(a => a.ToString()))}]" :
            Name;

    public bool HasArguments() => Arguments.Length > 0;

    public static readonly PythonTypeSpec Any = new("Any", []);
}
