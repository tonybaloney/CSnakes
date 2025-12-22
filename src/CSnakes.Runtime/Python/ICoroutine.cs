namespace CSnakes.Runtime.Python;

public interface ICoroutine;

public interface ICoroutine<T> : ICoroutine
{
    Task<T> AsTask(CancellationToken cancellationToken = default);
}
