using System.Collections;
using System.Collections.Immutable;
using PythonFunctionParameterListEntry = (CSnakes.Parser.Types.PythonFunctionParameterListEntryKind Kind, CSnakes.Parser.Types.PythonFunctionParameter Parameter);

namespace CSnakes.Parser.Types;
public enum PythonFunctionParameterListEntryKind
{
    Positional,
    Regular,
    VariadicPositional,
    Keyword,
    VariadicKeyword
}

public sealed class PythonFunctionParameterList(ImmutableArray<PythonFunctionParameter> positional = default,
                                                ImmutableArray<PythonFunctionParameter> regular = default,
                                                PythonFunctionParameter? varpos = null,
                                                ImmutableArray<PythonFunctionParameter> keyword = default,
                                                PythonFunctionParameter? varkw = null) :
    IReadOnlyList<PythonFunctionParameterListEntry>
{
    public static readonly PythonFunctionParameterList Empty = new();

    public ImmutableArray<PythonFunctionParameter> Positional { get; } = positional.IsDefault ? [] : positional;
    public ImmutableArray<PythonFunctionParameter> Regular { get; } = regular.IsDefault ? [] : regular;
    public PythonFunctionParameter? VariadicPositional { get; } = varpos;
    public ImmutableArray<PythonFunctionParameter> Keyword { get; } = keyword.IsDefault ? [] : keyword;
    public PythonFunctionParameter? VariadicKeyword { get; } = varkw;

    public PythonFunctionParameterList WithPositional(ImmutableArray<PythonFunctionParameter> value) =>
        value == Positional ? this : new(value.IsDefault ? [] : value, Regular, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList WithRegular(ImmutableArray<PythonFunctionParameter> value) =>
        value == Regular ? this : new(Positional, value.IsDefault ? [] : value, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList WithVariadicKeyword(PythonFunctionParameter? value) =>
        value == VariadicKeyword ? this : new(Positional, Regular, VariadicPositional, Keyword, value);

    private ImmutableArray<PythonFunctionParameterListEntry> all;

    private ImmutableArray<PythonFunctionParameterListEntry> All => all.IsDefault ? all = GetAll() : all;

    private ImmutableArray<PythonFunctionParameterListEntry> GetAll()
    {
        var count =
            Positional.Length
            + Regular.Length
            + (VariadicPositional is not null ? 1 : 0)
            + Keyword.Length + (VariadicKeyword is not null ? 1 : 0);

        var array = ImmutableArray.CreateBuilder<PythonFunctionParameterListEntry>(count);
        array.AddRange(from p in Positional select (PythonFunctionParameterListEntryKind.Positional, p));
        array.AddRange(from p in Regular select (PythonFunctionParameterListEntryKind.Regular, p));
        if (VariadicPositional is { } varpos)
            array.Add((PythonFunctionParameterListEntryKind.VariadicPositional, varpos));
        array.AddRange(from p in Keyword select (PythonFunctionParameterListEntryKind.Keyword, p));
        if (VariadicKeyword is { } varkw)
            array.Add((PythonFunctionParameterListEntryKind.VariadicKeyword, varkw));
        return array.MoveToImmutable();
    }

    public IEnumerator<PythonFunctionParameterListEntry> GetEnumerator() => All.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => All.Length;

    public PythonFunctionParameterListEntry this[int index] => All[index];
}

public abstract class PythonFunctionParameter
{
    protected PythonFunctionParameter(string name) => Name = name;

    public string Name { get; }

    public abstract class Regular : PythonFunctionParameter
    {
        private readonly PythonTypeSpec? type;

        protected Regular(string name, PythonTypeSpec? type, PythonConstant? defaultValue) : base(name)
        {
            this.type = type;
            DefaultValue = defaultValue;
        }

        public PythonTypeSpec Type => type ?? PythonTypeSpec.Any;
        public PythonConstant? DefaultValue { get; }
        public bool HasTypeAnnotation() => this.type is not null;
    }

    // Force a default value for *args and **kwargs as null, otherwise the calling convention is strange

    public sealed class Star(string name, PythonTypeSpec? type) :
        Regular(name, type, PythonConstant.None.Value);

    public sealed class DoubleStar(string name, PythonTypeSpec? type) :
        Regular(name, type, PythonConstant.None.Value);

    public sealed class Normal(string name, PythonTypeSpec? type, PythonConstant? defaultValue) :
        Regular(name, type, defaultValue);
}
