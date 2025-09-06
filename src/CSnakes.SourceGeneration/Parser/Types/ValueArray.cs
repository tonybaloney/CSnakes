using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace CSnakes.Parser.Types;

public static class ValueArray
{
    public static ValueArray<T> Create<T>(ReadOnlySpan<T> entries) => new([..entries]);
    public static ValueArray<T> Params<T>(params ReadOnlySpan<T> entries) => [..entries];
}

[CollectionBuilder(typeof(ValueArray), nameof(ValueArray.Create))]
public readonly record struct ValueArray<T> : IReadOnlyList<T>
{
    private readonly ImmutableArray<T> entries;

    public ValueArray(ImmutableArray<T> entries) => this.entries = entries;

    public ImmutableArray<T> Entries =>
        this.entries is { IsDefault: false } entries ? entries : [];

    public int Length => Entries.Length;
    int IReadOnlyCollection<T>.Count => Length;

    public T this[int index] => Entries[index];

    public override int GetHashCode()
    {
        var hash = 17L;
        foreach (var entry in this)
            hash = hash * 31 + (entry?.GetHashCode() ?? 0);
        return hash.GetHashCode();
    }

    public bool Equals(ValueArray<T> other) => Entries.SequenceEqual(other.Entries);

    public ImmutableArray<T>.Enumerator GetEnumerator() => Entries.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        IEnumerable<T> enumerable = Entries;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Entries;
        return enumerable.GetEnumerator();
    }

    public static implicit operator ValueArray<T>(ImmutableArray<T> entries) => new(entries);
    public static implicit operator ImmutableArray<T>(ValueArray<T> list) => list.Entries;
    public static implicit operator ReadOnlySpan<T>(ValueArray<T> list) => list.Entries.AsSpan();
}
