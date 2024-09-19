namespace CSnakes.Parser.Types;
public class PythonTypeSpec(string name, PythonTypeSpec[] arguments)
{
    public string Name { get; } = name;

    public PythonTypeSpec[] Arguments { get; } = arguments;

    public override string ToString() =>
        HasArguments() ?
            $"{Name}[{string.Join(", ", Arguments.Select(a => a.ToString()))}]" :
            Name;

    public bool HasArguments() => Arguments is not null && Arguments.Length > 0;

    public static PythonTypeSpec Any => new("Any", []);
}
