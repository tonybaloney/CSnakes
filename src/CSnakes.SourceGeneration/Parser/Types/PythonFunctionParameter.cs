global using PythonFunctionParameterList = CSnakes.Parser.Types.PythonFunctionParameterList<CSnakes.Parser.Types.PythonFunctionParameter.Positional, CSnakes.Parser.Types.PythonFunctionParameter.Normal, CSnakes.Parser.Types.PythonFunctionParameter.VariadicPositional, CSnakes.Parser.Types.PythonFunctionParameter.Keyword, CSnakes.Parser.Types.PythonFunctionParameter.VariadicKeyword>;
using System;
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
public sealed class PythonFunctionParameterList<TPositional, TRegular, TVariadicPositional, TKeyword, TVariadicKeyword>(
    ImmutableArray<TPositional> positional = default,
    ImmutableArray<TRegular> regular = default,
    (bool, TVariadicPositional) varpos = default,
    ImmutableArray<TKeyword> keyword = default,
    (bool, TVariadicKeyword) varkw = default)
{
    public static readonly PythonFunctionParameterList<TPositional, TRegular, TVariadicPositional, TKeyword, TVariadicKeyword> Empty = new();

    private string? stringRepresentation;

    public ImmutableArray<TPositional> Positional { get; } = positional.IsDefault ? [] : positional;
    public ImmutableArray<TRegular> Regular { get; } = regular.IsDefault ? [] : regular;
    public (bool, TVariadicPositional) VariadicPositional { get; } = varpos;
    public ImmutableArray<TKeyword> Keyword { get; } = keyword.IsDefault ? [] : keyword;
    public (bool, TVariadicKeyword) VariadicKeyword { get; } = varkw;

    public PythonFunctionParameterList<TPositional, TRegular, TVariadicPositional, TKeyword, TVariadicKeyword>
        WithPositional(ImmutableArray<TPositional> value) =>
        new(value.IsDefault ? [] : value, Regular, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList<TPositional, TRegular, TVariadicPositional, TKeyword, TVariadicKeyword>
        WithRegular(ImmutableArray<TRegular> value) =>
        new(Positional, value.IsDefault ? [] : value, VariadicPositional, Keyword, VariadicKeyword);

    public PythonFunctionParameterList<TPositional, TRegular, TVariadicPositional, TKeyword, TVariadicKeyword>
        WithVariadicPositional((bool, TVariadicPositional) value) =>
        new(Positional, Regular, value, Keyword, VariadicKeyword);

    public PythonFunctionParameterList<TPositional, TRegular, TVariadicPositional, TKeyword, TVariadicKeyword>
        WithVariadicKeyword((bool, TVariadicKeyword) value) =>
        new(Positional, Regular, VariadicPositional, Keyword, value);

    public IEnumerable<T> Enumerable<T>(Func<TPositional, T> positionalProjector,
                                        Func<TRegular, T> regularProjector,
                                        Func<TVariadicPositional, T> variadicPositionalProjector,
                                        Func<TKeyword, T> keywordProjector,
                                        Func<TVariadicKeyword, T> variadicKeywordProjector)
    {
        foreach (var p in Positional)
            yield return positionalProjector(p);

        foreach (var p in Regular)
            yield return regularProjector(p);

        if (VariadicPositional is (true, var vpp))
            yield return variadicPositionalProjector(vpp);

        foreach (var p in Keyword)
            yield return keywordProjector(p);

        if (VariadicKeyword is (true, var vkp))
            yield return variadicKeywordProjector(vkp);
    }

    public PythonFunctionParameterList<TPositional2, TRegular2, TVariadicPositional2, TKeyword2, TVariadicKeyword2>
        Map<TPositional2, TRegular2, TVariadicPositional2, TKeyword2, TVariadicKeyword2>(
            Func<TPositional, TPositional2> positionalProjector,
            Func<TRegular, TRegular2> regularProjector,
            Func<TVariadicPositional, TVariadicPositional2> variadicPositionalProjector,
            Func<TKeyword, TKeyword2> keywordProjector,
            Func<TVariadicKeyword, TVariadicKeyword2> variadicKeywordProjector) =>
        new([..Positional.Select(positionalProjector)],
            [..Regular.Select(regularProjector)],
            VariadicPositional is (true, var vpp) ? (true, variadicPositionalProjector(vpp)) : default,
            [..Keyword.Select(keywordProjector)],
            VariadicKeyword is (true, var vkp) ? (true, variadicKeywordProjector(vkp)) : default);

    public int Count => Positional.Length
                        + Regular.Length
                        + (VariadicPositional is (true, _) ? 1 : 0)
                        + Keyword.Length
                        + (VariadicKeyword is (true, _) ? 1 : 0);

    public override string ToString() => stringRepresentation ??= BuildString();

    private string BuildString()
    {
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

        if (VariadicPositional is (true, var vpp))
            _ = Delimit().Append('*').Append(vpp);

        if (Keyword.Any())
        {
            _ = Delimit().Append('*');
            foreach (var p in Keyword)
                _ = sb.Append(", ").Append(p);
        }

        if (VariadicKeyword is (true, var vkp))
            _ = Delimit().Append("**").Append(vkp);

        return sb.Append(")").ToString();
    }
}

public static class PythonFunctionParameterListExtensions
{
    public static IEnumerable<T> Enumerable<T>(this PythonFunctionParameterList<T, T, T, T, T> list) =>
        list.Enumerable(x => x, x => x, x => x, x => x, x => x);

    public static IEnumerable<PythonFunctionParameter> Enumerable(this PythonFunctionParameterList list) =>
        list.Enumerable(x => (PythonFunctionParameter)x, x => x, x => x, x => x, x => x);
}
