namespace CSnakes.Parser;
internal static partial class EnumerableExtensions
{
    /// <summary>
    /// Just like <see
    /// href="https://docs.python.org/3/library/itertools.html#itertools.pairwise"><c>itertools.pairwise</c></see>
    /// in Python, returns a sequence of adjacent pairs of elements from the
    /// input sequence.
    /// </summary>
    /// <remarks>
    /// The number of pairs yielded will be one less than the number of elements
    /// in <paramref name="source"/>. It will be empty if <paramref
    /// name="source"/> has fewer than two elements.
    /// </remarks>
    public static IEnumerable<(T First, T Second)> Pairwise<T>(this IEnumerable<T> source)
    {
        return source is null ? throw new ArgumentNullException(nameof(source)) : Iterator(source);

        static IEnumerable<(T First, T Second)> Iterator(IEnumerable<T> source)
        {
            using var e = source.GetEnumerator();

            if (!e.MoveNext())
                yield break;

            T previous = e.Current;
            while (e.MoveNext())
            {
                yield return (previous, e.Current);
                previous = e.Current;
            }
        }
    }

    public static IEnumerable<int[]> Permutations(this IEnumerable<int> ranges)
    {
        // Given a list of ranges, e.g.
        // [2, 3, 1] (2 options for first, 3 for second, 1 for third),
        // return all permutations of indexes, 0-indexed
        // e.g.[2,3,1].Permutations() == [0, 0, 0], [0, 1, 0], [0, 2, 0], [1, 0, 0], [1, 1, 0], [1, 2, 0]

        // Convert to array for index access
        var rangeArray = ranges.ToArray();
        if (rangeArray.Length == 0)
            yield break;

        if (rangeArray.Any(r => r < 1))
            throw new ArgumentException("All ranges must be at least 1.", nameof(ranges));

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

    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item) =>
        source.Concat(Enumerable.Repeat(item, 1));
}
