using System.Collections.Immutable;

namespace CSnakes.Parser.Types;

public record PythonTypeSpec(string Name)
{
    public static readonly AnyType Any = AnyType.Instance;
    public static readonly NoneType None = NoneType.Instance;
    public static readonly IntType Int = IntType.Instance;
    public static readonly StrType Str = StrType.Instance;
    public static readonly FloatType Float = FloatType.Instance;
    public static readonly BoolType Bool = BoolType.Instance;
    public static readonly BytesType Bytes = BytesType.Instance;
    public static readonly BufferType Buffer = BufferType.Instance;

    public override string ToString() => Name;
}

public sealed record NoneType : PythonTypeSpec
{
    public static readonly NoneType Instance = new();
    private NoneType() : base("NoneType") { }
    public override string ToString() => Name;
}

public sealed record AnyType : PythonTypeSpec
{
    public static readonly AnyType Instance = new();
    private AnyType() : base("Any") { }
    public override string ToString() => Name;
}

public sealed record IntType : PythonTypeSpec
{
    public static readonly IntType Instance = new();
    private IntType() : base("int") { }
    public override string ToString() => Name;
}

public sealed record StrType : PythonTypeSpec
{
    public static readonly StrType Instance = new();
    private StrType() : base("str") { }
    public override string ToString() => Name;
}

public sealed record FloatType : PythonTypeSpec
{
    public static readonly FloatType Instance = new();
    private FloatType() : base("float") { }
    public override string ToString() => Name;
}

public sealed record BoolType : PythonTypeSpec
{
    public static readonly BoolType Instance = new();
    private BoolType() : base("bool") { }
    public override string ToString() => Name;
}

public sealed record BytesType : PythonTypeSpec
{
    public static readonly BytesType Instance = new();
    private BytesType() : base("bytes") { }
    public override string ToString() => Name;
}

public sealed record BufferType : PythonTypeSpec
{
    public static readonly BufferType Instance = new();
    private BufferType() : base("Buffer") { }
    public override string ToString() => Name;
}

public abstract record ClosedGenericType(string Name) : PythonTypeSpec(Name);

public interface ISequenceType
{
    PythonTypeSpec Of { get; }
}

public sealed record SequenceType(PythonTypeSpec Of) : ClosedGenericType("Sequence"), ISequenceType
{
    public override string ToString() => $"{Name}[{Of}]";
}

public sealed record ListType(PythonTypeSpec Of) : ClosedGenericType("list"), ISequenceType
{
    public override string ToString() => $"{Name}[{Of}]";
}

public interface IMappingType
{
    PythonTypeSpec Key { get; }
    PythonTypeSpec Value { get; }
}

public sealed record MappingType(PythonTypeSpec Key, PythonTypeSpec Value) : ClosedGenericType("Mapping"), IMappingType
{
    public override string ToString() => $"{Name}[{Key}, {Value}]";
}

public sealed record DictType(PythonTypeSpec Key, PythonTypeSpec Value) : ClosedGenericType("dict"), IMappingType
{
    public override string ToString() => $"{Name}[{Key}, {Value}]";
}

public sealed record CoroutineType(PythonTypeSpec Yield, PythonTypeSpec Send, PythonTypeSpec Return) : ClosedGenericType("Coroutine")
{
    public override string ToString() => $"{Name}[{Yield}, {Send}, {Return}]";
}

public sealed record GeneratorType(PythonTypeSpec Yield, PythonTypeSpec Send, PythonTypeSpec Return) : ClosedGenericType("Generator")
{
    public override string ToString() => $"{Name}[{Yield}, {Send}, {Return}]";
}

public sealed record LiteralType(ValueArray<PythonConstant> Constants) : PythonTypeSpec("Literal")
{
    public override string ToString()
    {
        var constants =
            from c in Constants
            select c switch
            {
                PythonConstant.Float { Value: var v } => FormattableString.Invariant($"{v:0.0}"),
                PythonConstant.String { Value: var v } => $"'{v}'",
                var other => other.ToString(),
            };
        return $"{Name}[{string.Join(", ", constants)}]";
    }
}

public sealed record OptionalType(PythonTypeSpec Of) : ClosedGenericType("Optional")
{
    public override string ToString() => $"{Name}[{Of}]";
}

public sealed record CallbackType(ValueArray<PythonTypeSpec> Parameters, PythonTypeSpec Return) : ClosedGenericType("Callback")
{
    public override string ToString() => $"{Name}[[{Parameters}], {Return}]";
}

public sealed record TupleType(ValueArray<PythonTypeSpec> Parameters) : ClosedGenericType("tuple")
{
    public override string ToString() => $"{Name}[{string.Join(", ", Parameters)}]";
}

public sealed record VariadicTupleType(PythonTypeSpec Of) : PythonTypeSpec("tuple")
{
    public override string ToString() => $"{Name}[{Of}, ...]";
}

public sealed record UnionType(ValueArray<PythonTypeSpec> Choices) : ClosedGenericType("Union")
{
    public override string ToString() => $"{Name}[{string.Join(", ", Choices)}]";

    public static PythonTypeSpec Normalize(ReadOnlySpan<PythonTypeSpec> choices)
    {
        if (choices.IsEmpty)
            throw new ArgumentException("Union must have at least one type argument.", nameof(choices));

        switch (choices)
        {
            // Union of a single type is just that type
            case [not UnionType and var arg]: return arg;
            // Union of a single type with None is Optional of that type
            case [not NoneType and not UnionType and not OptionalType and var arg, NoneType]: return new OptionalType(arg);
            // Union of None with a single type is Optional of that type
            case [NoneType, not NoneType and not UnionType and not OptionalType and var arg]: return new OptionalType(arg);
            // Optimise for common cases of Union with two...
            case [not UnionType and not OptionalType and var a, not UnionType and not OptionalType and var b]
                when a != b: return new UnionType([a, b]);
            // ...or three different types
            case [not UnionType and not OptionalType and var a, not UnionType and not OptionalType and var b, not UnionType and not OptionalType and var c]
                when a != b && a != c && b != c:
                return new UnionType([a, b, c]);
            default:
            {
                var list = ImmutableArray.CreateBuilder<PythonTypeSpec>(); // unique in order of appearance
                const int setThreshold = 8; // # of argument after which to switch to a set for performance
                HashSet<PythonTypeSpec>? set = null; // set used for containment tests for large unions

                void Flatten(ReadOnlySpan<PythonTypeSpec> args)
                {
                    foreach (var t in args)
                    {
                        switch (t)
                        {
                            case UnionType { Choices: var cs }: Flatten(cs); break;
                            case OptionalType { Of: var of }: Flatten([of, NoneType.Instance]); break;
                            default:
                            {
                                if (set?.Add(t) ?? !list.Contains(t))
                                {
                                    list.Add(t);
                                    if (list.Count == setThreshold)
                                        set = [..list];
                                }
                                break;
                            }
                        }
                    }
                }

                Flatten(choices);

                return list switch
                {
                    // A type and None, is Optional of that type
                    [not NoneType and var arg, NoneType] => new OptionalType(arg),
                    // None and a type, is Optional of that type
                    [NoneType, not NoneType and var arg] => new OptionalType(arg),
                    // A single type, is just that type
                    [var arg] => arg,
                    var args => new UnionType(args.ToImmutable())
                };
            }
        }
    }
}

public sealed record GenericPythonTypeSpec(string Name, ValueArray<PythonTypeSpec> Arguments) : ClosedGenericType(Name)
{
    public override string ToString() =>
        this switch
        {
            { Name: var name, Arguments: [] } => name,
            { Name: var name, Arguments: [var arg] } => $"{name}[{arg}]",
            { Name: var name, Arguments: var args } => $"{name}[{string.Join(", ", args)}]"
        };
}
