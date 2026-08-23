using CSnakes.Runtime.CPython;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

[RequiresDynamicCode(DynamicCodeMessages.CallsMakeGenericType)]
internal sealed class Awaitable<T>(PyObject awaitable) :
    Awaitable<T, PyObjectImporters.Runtime<T>>(awaitable);

public static class Awaitable
{
    /// <summary>
    /// Asynchronously waits for the given awaitable to complete. An additional argument specifies
    /// whether to dispose the awaitable after the wait is over.
    /// </summary>
    public static async Task<T> WaitAsync<T>(this IAwaitable<T> awaitable, bool dispose,
                                             CancellationToken cancellationToken = default)
    {
        try
        {
            return await awaitable.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (dispose)
            {
                using (GIL.Acquire())
                    awaitable.Dispose();
            }
        }
    }

    /// <summary>
    /// Asynchronously waits for the given Python <see
    /// href="https://docs.python.org/3/library/collections.abc.html#collections.abc.Awaitable"><c>collections.abc.Awaitable</c></see>
    /// to complete and returns the result.
    /// </summary>
    /// <param name="obj">The <c>Awaitable</c> Python object.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous wait operation. The task result contains the awaited
    /// value.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="obj"/> is not a Python
    /// awaitable.</exception>
    public static Task<PyObject> WaitAsync(PyObject obj, CancellationToken cancellationToken)
    {
        using (GIL.Acquire())
        {
            if (!CPythonAPI.IsPyAwaitable(obj))
                throw new ArgumentException($"Expected an awaitable (collections.abc.Awaitable), but got {obj.GetPythonType()}", nameof(obj));
        }

        return InternalWaitAsync(obj, cancellationToken);
    }

    /// <summary>
    /// Same as <see cref="WaitAsync"/> except <paramref name="awaitable"/> is assumed to have been
    /// checked to be a Python <c>Awaitable</c>.
    /// </summary>
    internal static async Task<PyObject> InternalWaitAsync(PyObject awaitable, CancellationToken cancellationToken)
    {
        Task<PyObject> task;

        using (GIL.Acquire())
            task = CPythonAPI.GetDefaultEventLoop().RunAsync(awaitable, cancellationToken);

        return await task.ConfigureAwait(false);
    }
}

internal class Awaitable<T, TImporter>(PyObject awaitable) :
    IAwaitable<T>
    where TImporter : IPyObjectImporter<T>
{
    async Task<PyObject> IAwaitable.WaitAsync(CancellationToken cancellationToken) =>
        await Awaitable.InternalWaitAsync(awaitable, cancellationToken).ConfigureAwait(false);

    public async Task<T> WaitAsync(CancellationToken cancellationToken = default)
    {
        using var result = await Awaitable.InternalWaitAsync(awaitable, cancellationToken).ConfigureAwait(false);
        using (GIL.Acquire())
            return TImporter.BareImport(result);
    }

    public void Dispose() => awaitable.Dispose();

    PyObject IPyObjectProxy.DangerousInternalReference => awaitable;
}
