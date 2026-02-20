namespace CSnakes.Runtime.Python;

/// <summary>
/// Represents a Python <see
/// href="https://docs.python.org/3/library/collections.abc.html#collections.abc.Awaitable"><c>collections.abc.Awaitable[Any]</c></see>.
/// </summary>
public interface IAwaitable : IPyObjectProxy
{
    /// <summary>
    /// Waits asynchronously for the Python awaitable to complete and returns the result.
    /// </summary>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests. The default is <see
    /// cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous wait operation. The task result contains the awaited
    /// value.
    /// </returns>
    Task<PyObject> WaitAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a generic Python <see
/// href="https://docs.python.org/3/library/collections.abc.html#collections.abc.Awaitable"><c>collections.abc.Awaitable</c></see>
/// that returns an <em>imported</em> value of the type argument when awaited.
/// </summary>
public interface IAwaitable<T> : IAwaitable
{
    /// <summary>
    /// Waits asynchronously for the Python awaitable to complete and returns the result.
    /// </summary>
    /// <param name="cancellationToken">
    /// The token to monitor for cancellation requests. The default is <see
    /// cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous wait operation. The task result contains the awaited
    /// value.
    /// </returns>
    new Task<T> WaitAsync(CancellationToken cancellationToken = default);
}
