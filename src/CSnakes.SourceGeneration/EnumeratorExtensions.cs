namespace CSnakes;
static class EnumeratorExtensions
{
    public static T Read<T>(this IEnumerator<T> enumerator)
    {
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("Enumerator has no more elements.");
        return enumerator.Current;
    }
}
