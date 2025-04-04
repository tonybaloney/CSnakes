namespace CSnakes.Parser.Types;
public class PythonTypeSpec(string name, PythonTypeSpec[] arguments)
{
    public string Name { get; } = name;

    public PythonTypeSpec[] Arguments { get; } = arguments;

    public override string ToString() =>
        Arguments is { Length: > 0 } args ?
            $"{Name}[{string.Join(", ", args.Select(a => a.ToString()))}]" :
            Name;

    public static PythonTypeSpec Any => new("Any", []);
}
