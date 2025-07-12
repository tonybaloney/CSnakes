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

    public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item) =>
        source.Concat(Enumerable.Repeat(item, 1));
}
