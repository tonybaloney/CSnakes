namespace CSnakes.Runtime.Python;

public interface IAsyncIterator <out TYield> : IAsyncEnumerator<TYield>, IAsyncEnumerable<TYield>
{
}

