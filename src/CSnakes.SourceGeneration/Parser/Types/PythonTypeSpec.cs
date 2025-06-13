using System.Collections.Immutable;

namespace CSnakes.Parser.Types;
public sealed class PythonTypeSpec(string name, ImmutableArray<PythonTypeSpec> arguments = default) :
    IEquatable<PythonTypeSpec>
{
    private int? cachedHashCode;

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

    public static PythonTypeSpec Union(ReadOnlySpan<PythonTypeSpec> arguments)
    {
        if (arguments.IsEmpty)
            throw new ArgumentException("Union must have at least one type argument.", nameof(arguments));

        switch (arguments)
        {
            // Union of a single type is just that type
            case [{ Name: not "Union" } arg]: return arg;
            // Union of a single type with None is Optional of that type
            case [{ Name: not "None" and not "Union" and not "Optional" } arg, { Name: "None" }]: return Optional(arg);
            // Union of None with a single type is Optional of that type
            case [{ Name: "None" }, { Name: not "None" and not "Union" and not "Optional" } arg]: return Optional(arg);
            // Optimise for common cases of Union with two...
            case [{ Name: not "Union" and not "Optional" } a, { Name: not "Union" and not "Optional" } b]
                when a != b: return new("Union", [a, b]);
            // ...or three different types
            case [{ Name: not "Union" and not "Optional" } a, { Name: not "Union" and not "Optional" } b, { Name: not "Union" and not "Optional" } c]
                when a != b && a != c && b != c:
                return new("Union", [a, b, c]);
            default:
            {
                var list = ImmutableArray.CreateBuilder<PythonTypeSpec>(); // unique in order of appearance
                const int setThreshold = 8; // # of argument after which to switch to a set for performance
                HashSet<PythonTypeSpec>? set = null; // set used for containment tests for large unions

                void Flatten(ReadOnlySpan<PythonTypeSpec> args)
                {
                    foreach (var t in args)
                    {
                        switch (t.Name)
                        {
                            case "Union": Flatten(t.Arguments.AsSpan()); break;
                            case "Optional": Flatten([t.Arguments[0], None]); break;
                            default:
                            {
                                if (set?.Add(t) ?? list?.Contains(t) is null or false)
                                {
                                    list ??= ImmutableArray.CreateBuilder<PythonTypeSpec>();
                                    list.Add(t);
                                    if (list.Count == setThreshold)
                                        set = [..list];
                                }
                                break;
                            }
                        }
                    }
                }

                Flatten(arguments);

                return list switch
                {
                    // A type and None, is Optional of that type
                    [{ Name: not "None" } arg, { Name: "None" }] => Optional(arg),
                    // None and a type, is Optional of that type
                    [{ Name: "None" }, { Name: not "None" } arg] => Optional(arg),
                    // A single type, is just that type
                    [var arg] => arg,
                    var args => new("Union", args.ToImmutable())
                };
            }
        }
    }

    public bool Equals(PythonTypeSpec? other) =>
        other is not null && (ReferenceEquals(this, other) || Name == other.Name && Arguments.SequenceEqual(other.Arguments));

    public override bool Equals(object? obj) =>
        Equals(obj as PythonTypeSpec);

    public static bool operator ==(PythonTypeSpec? left, PythonTypeSpec? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(PythonTypeSpec? left, PythonTypeSpec? right) =>
        !(left == right);

    public override int GetHashCode()
    {
        if (this.cachedHashCode is { } someHashCode)
            return someHashCode;

        unchecked
        {
            var hash = Name.GetHashCode() * 397;
            foreach (var argument in Arguments)
                hash ^= argument.GetHashCode();
            this.cachedHashCode = hash;
            return hash;
        }
    }
}
