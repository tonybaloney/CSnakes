global using PythonFunctionParameterList = CSnakes.Parser.Types.PythonFunctionParameterList<CSnakes.Parser.Types.PythonFunctionParameter>;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace CSnakes.Parser.Types;

public sealed class PythonFunctionParameter(string name, PythonTypeSpec? type, PythonConstant? defaultValue)
{
    public string Name { get; } = name;
    public PythonTypeSpec? TypeSpec => type;
    public PythonTypeSpec ImpliedTypeSpec => type ?? PythonTypeSpec.Any;
    public PythonConstant? DefaultValue { get; } = defaultValue;

    public PythonFunctionParameter WithDefaultValue(PythonConstant? value) =>
        value == DefaultValue ? this : new(Name, type, value);

    public override string ToString() =>
        this switch
        {
            { TypeSpec: null  , DefaultValue: null   } => Name,
            { TypeSpec: { } ts, DefaultValue: null   } => $"{Name}: {ts}",
            { TypeSpec: { } ts, DefaultValue: { } dv } => $"{Name}: {ts} = {dv}",
            { TypeSpec: null  , DefaultValue: { } dv } => $"{Name} = {dv}",
        };
}

/// <remarks>
/// The order of the parameters is inspired by <see
/// href="https://docs.python.org/3/library/ast.html#ast.arguments"><c>ast.arguments</c></see>.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed class PythonFunctionParameterList<T>(ImmutableArray<T> positional = default,
                                                   ImmutableArray<T> regular = default,
                                                   T? varpos = default,
                                                   ImmutableArray<T> keyword = default,
                                                   T? varkw = default)
{
    public static readonly PythonFunctionParameterList<T> Empty = new();

    public ImmutableArray<T> Positional { get; } = positional.IsDefault ? [] : positional;
    public ImmutableArray<T> Regular { get; } = regular.IsDefault ? [] : regular;
    public T? VariadicPositional { get; } = varpos;
    public ImmutableArray<T> Keyword { get; } = keyword.IsDefault ? [] : keyword;
    public T? VariadicKeyword { get; } = varkw;

    public PythonFunctionParameterList<T>
        WithPositional(ImmutableArray<T> value) =>
        new(value.IsDefault ? [] : value, Regular, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList<T>
        WithRegular(ImmutableArray<T> value) =>
        new(Positional, value.IsDefault ? [] : value, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList<T>
        WithVariadicPositional(T? value) =>
        new(Positional, Regular, value, Keyword, VariadicKeyword);

    public PythonFunctionParameterList<T>
        WithVariadicKeyword(T? value) =>
        new(Positional, Regular, VariadicPositional, Keyword, value);

    public IEnumerable<TResult> Enumerable<TResult>(Func<T, TResult> positionalProjector,
                                                    Func<T, TResult> regularProjector,
                                                    Func<T, TResult> variadicPositionalProjector,
                                                    Func<T, TResult> keywordProjector,
                                                    Func<T, TResult> variadicKeywordProjector)
    {
        foreach (var p in Positional)
            yield return positionalProjector(p);

        foreach (var p in Regular)
            yield return regularProjector(p);

        if (VariadicPositional is { } vpp)
            yield return variadicPositionalProjector(vpp);

        foreach (var p in Keyword)
            yield return keywordProjector(p);

        if (VariadicKeyword is { } vkp)
            yield return variadicKeywordProjector(vkp);
    }

    public PythonFunctionParameterList<TResult>
        Map<TResult>(Func<T, TResult> positionalProjector,
                     Func<T, TResult> regularProjector,
                     Func<T, TResult> variadicPositionalProjector,
                     Func<T, TResult> keywordProjector,
                     Func<T, TResult> variadicKeywordProjector)
        where TResult : class =>
        new([..Positional.Select(positionalProjector)],
            [..Regular.Select(regularProjector)],
            VariadicPositional is { } vpp ? variadicPositionalProjector(vpp) : null,
            [..Keyword.Select(keywordProjector)],
            VariadicKeyword is { } vkp ? variadicKeywordProjector(vkp) : null);

    public int Count => Positional.Length
                        + Regular.Length
                        + (VariadicPositional is not null ? 1 : 0)
                        + Keyword.Length
                        + (VariadicKeyword is not null ? 1 : 0);

    private string DebuggerDisplay()
    {
        if (typeof(T) != typeof(PythonFunctionParameter))
            return ToString();

        var sb = new StringBuilder("(");

        if (Positional.Any())
        {
            foreach (var p in Positional)
                _ = sb.Append(p).Append(", ");
            _ = sb.Append("/");
        }

        StringBuilder Delimit() => sb.Length > 1 ? sb.Append(", ") : sb;

        foreach (var p in Regular)
            _ = Delimit().Append(p);

        if (VariadicPositional is { } vpp)
            _ = Delimit().Append('*').Append(vpp);

        if (Keyword.Any())
        {
            _ = Delimit().Append('*');
            foreach (var p in Keyword)
                _ = sb.Append(", ").Append(p);
        }

        if (VariadicKeyword is { } vkp)
            _ = Delimit().Append("**").Append(vkp);

        return sb.Append(")").ToString();
    }
}

public static class PythonFunctionParameterListExtensions
{
    public static IEnumerable<T> Enumerable<T>(this PythonFunctionParameterList<T> list)
        where T : class =>
        list.Enumerable(x => x, x => x, x => x, x => x, x => x);
}
