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
#pragma warning disable format
            { TypeSpec: null  , DefaultValue: null   } => Name,
            { TypeSpec: { } ts, DefaultValue: null   } => $"{Name}: {ts}",
            { TypeSpec: { } ts, DefaultValue: { } dv } => $"{Name}: {ts} = {dv}",
            { TypeSpec: null  , DefaultValue: { } dv } => $"{Name} = {dv}",
#pragma warning restore format
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
        new([.. Positional.Select(positionalProjector)],
            [.. Regular.Select(regularProjector)],
            VariadicPositional is { } vpp ? variadicPositionalProjector(vpp) : null,
            [.. Keyword.Select(keywordProjector)],
            VariadicKeyword is { } vkp ? variadicKeywordProjector(vkp) : null);

    public static IEnumerable<int[]> Permutations(IEnumerable<int> ranges)
    {
        // Given a list of ranges, e.g.
        // [2, 3, 1] (2 options for first, 3 for second, 1 for third), return all combinations of indexes
        // e.g. [0, 0, 0], [0, 0, 1], [0, 1, 0], [0, 1, 1], [1, 0, 0], [1, 0, 1], [1, 1, 0], [1, 1, 1]

        // Convert to array for index access
        var rangeArray = ranges.ToArray();
        if (rangeArray.Length == 0)
            yield break;

        var indices = new int[rangeArray.Length];
        while (true)
        {
            // Yield a copy of the current indices
            yield return (int[])indices.Clone();

            // Increment indices like an odometer
            int pos = rangeArray.Length - 1;
            while (pos >= 0)
            {
                indices[pos]++;
                if (indices[pos] < rangeArray[pos])
                    break;
                indices[pos] = 0;
                pos--;
            }
            if (pos < 0)
                break;
        }
    }

    public IEnumerable<PythonFunctionParameterList<TResult>>
        MapMany<TResult>(Func<T, IEnumerable<TResult>> positionalProjector,
                         Func<T, IEnumerable<TResult>> regularProjector,
                         Func<T, IEnumerable<TResult>> variadicPositionalProjector,
                         Func<T, IEnumerable<TResult>> keywordProjector,
                         Func<T, IEnumerable<TResult>> variadicKeywordProjector)
        where TResult : class
    {
        /* Each parameter group will be a list of parameters with a list of potential types.
         * 
         * | Positional        | Regular         | VariadicPositional | Keyword        | VariadicKeyword  |
         * |-------------------|-----------------|--------------------|----------------|------------------|
         * | 0: [type1, type2] | [type3]         | ...                | [type5, type6] | [type7]          |
         * | 1: [type8]        |                 |                    | [type12]       |                  |
         * 
         * etc. 
         */
        // Skip all this malarky if there are no parameters at all
        if (Positional.IsEmpty && Regular.IsEmpty && VariadicPositional is not { } && Keyword.IsEmpty && VariadicKeyword is not { })
        {
            yield return new PythonFunctionParameterList<TResult>();
            yield break;
        }

        var positionalOptions = Positional.Select(positionalProjector);
        var regularOptions = Regular.Select(regularProjector);
        var variadicOption = VariadicPositional is { } vpp ? variadicPositionalProjector(vpp) : null;
        var keywordOptions = Keyword.Select(keywordProjector);
        var variadicKeywordOption = VariadicKeyword is { } vkp ? variadicKeywordProjector(vkp) : null;

        List<int> ranges = [
            .. positionalOptions.Select(x => x.Count()),
            .. regularOptions.Select(x => x.Count()),
        ];
        if (variadicOption is not null)
            ranges.Add(variadicOption.Count());
        ranges.AddRange(keywordOptions.Select(x => x.Count()));
        if (variadicKeywordOption is not null)
            ranges.Add(variadicKeywordOption.Count());

        foreach (var turn in Permutations(ranges)) {
            List<TResult> positionalTurn = new(positionalOptions.Count());
            for (int i = 0; i < positionalOptions.Count(); i++)
                positionalTurn.Add(positionalOptions.ElementAt(i).ElementAt(turn[i]));
            List<TResult> regularTurn = new(regularOptions.Count());
            for (int i = 0; i < regularOptions.Count(); i++)
                regularTurn.Add(regularOptions.ElementAt(i).ElementAt(turn[Positional.Length + i]));
            TResult? variadicPositionalTurn = null;
            if (variadicOption is not null)
                variadicPositionalTurn = variadicOption.ElementAt(turn[Positional.Length + Regular.Length]);
            List<TResult> keywordTurn = new(keywordOptions.Count());
            for (int i = 0; i < keywordOptions.Count(); i++)
                keywordTurn.Add(keywordOptions.ElementAt(i).ElementAt(turn[Positional.Length + Regular.Length + (VariadicPositional is not null ? 1 : 0) + i]));
            TResult? variadicKeywordTurn = null;
            if (variadicKeywordOption is not null)
                variadicKeywordTurn = variadicKeywordOption.ElementAt(turn[^1]);

            yield return new PythonFunctionParameterList<TResult>(
                [.. positionalTurn],
                [.. regularTurn],
                variadicPositionalTurn,
                [.. keywordTurn],
                variadicKeywordTurn
                );
        }
    }


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
