using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace CSnakes.Parser.Types;
public abstract class PythonFunctionParameter(string name, PythonTypeSpec? type, PythonConstant? defaultValue)
{
    public string Name { get; } = name;
    public PythonTypeSpec Type => type ?? PythonTypeSpec.Any;
    public PythonConstant? DefaultValue { get; } = defaultValue;
    public bool HasTypeAnnotation() => type is not null;

    public abstract class Unit(string name, PythonTypeSpec? type, PythonConstant? defaultValue) :
        PythonFunctionParameter(name, type, defaultValue);

    public abstract class Variadic(string name, PythonTypeSpec? type) :
        PythonFunctionParameter(name, type, PythonConstant.None.Value);

    public sealed class Positional(string name, PythonTypeSpec? type, PythonConstant? defaultValue) :
        Unit(name, type, defaultValue);

    public sealed class Normal(string name, PythonTypeSpec? type, PythonConstant? defaultValue) :
        Unit(name, type, defaultValue);

    public sealed class VariadicPositional(string name, PythonTypeSpec? type) :
        Variadic(name, type);

    public sealed class Keyword(string name, PythonTypeSpec? type, PythonConstant? defaultValue) :
        Unit(name, type, defaultValue);

    public sealed class VariadicKeyword(string name, PythonTypeSpec? type) :
        Variadic(name, type);
}

/// <remarks>
/// The order of the parameters is inspired by <see
/// href="https://docs.python.org/3/library/ast.html#ast.arguments"><c>ast.arguments</c></see>.
/// </remarks>
public sealed class PythonFunctionParameterList(ImmutableArray<PythonFunctionParameter.Positional> positional = default,
                                                ImmutableArray<PythonFunctionParameter.Normal> regular = default,
                                                PythonFunctionParameter.VariadicPositional? varpos = null,
                                                ImmutableArray<PythonFunctionParameter.Keyword> keyword = default,
                                                PythonFunctionParameter.VariadicKeyword? varkw = null) :
    IReadOnlyList<PythonFunctionParameter>
{
    public static readonly PythonFunctionParameterList Empty = new();

    private ImmutableArray<PythonFunctionParameter> all;
    private string? stringRepresentation;

    public ImmutableArray<PythonFunctionParameter.Positional> Positional { get; } = positional.IsDefault ? [] : positional;
    public ImmutableArray<PythonFunctionParameter.Normal> Regular { get; } = regular.IsDefault ? [] : regular;
    public PythonFunctionParameter.VariadicPositional? VariadicPositional { get; } = varpos;
    public ImmutableArray<PythonFunctionParameter.Keyword> Keyword { get; } = keyword.IsDefault ? [] : keyword;
    public PythonFunctionParameter.VariadicKeyword? VariadicKeyword { get; } = varkw;

    public PythonFunctionParameterList WithPositional(ImmutableArray<PythonFunctionParameter.Positional> value) =>
        value == Positional ? this : new(value.IsDefault ? [] : value, Regular, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList WithRegular(ImmutableArray<PythonFunctionParameter.Normal> value) =>
        value == Regular ? this : new(Positional, value.IsDefault ? [] : value, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList WithVariadicKeyword(PythonFunctionParameter.VariadicKeyword? value) =>
        value == VariadicKeyword ? this : new(Positional, Regular, VariadicPositional, Keyword, value);

    private ImmutableArray<PythonFunctionParameter> All => all.IsDefault ? all = [..this] : all;

    public IEnumerator<PythonFunctionParameter> GetEnumerator()
    {
        foreach (var p in Positional)
            yield return p;

        foreach (var p in Regular)
            yield return p;

        if (VariadicPositional is { } vpp)
            yield return vpp;

        foreach (var p in Keyword)
            yield return p;

        if (VariadicKeyword is { } vkp)
            yield return vkp;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Positional.Length
                        + Regular.Length
                        + (VariadicPositional is not null ? 1 : 0)
                        + Keyword.Length
                        + (VariadicKeyword is not null ? 1 : 0);

    public PythonFunctionParameter this[int index] => All[index];

    public override string ToString() => stringRepresentation ??= BuildString();

    private string BuildString()
    {
        var sb = new StringBuilder("(");

        if (Positional.Any())
        {
            foreach (var p in Positional)
                _ = sb.Append(p.Name).Append(", ");
            _ = sb.Append("/");
        }

        StringBuilder Delimit() => sb.Length > 1 ? sb.Append(", ") : sb;

        foreach (var p in Regular)
            _ = Delimit().Append(p.Name);

        if (VariadicPositional is { } vpp)
            _ = Delimit().Append('*').Append(vpp.Name);

        if (Keyword.Any())
        {
            _ = Delimit().Append('*');
            foreach (var p in Keyword)
                _ = sb.Append(", ").Append(p.Name);
        }

        if (VariadicKeyword is { } vkp)
            _ = Delimit().Append("**").Append(vkp.Name);

        return sb.Append(")").ToString();
    }
}
