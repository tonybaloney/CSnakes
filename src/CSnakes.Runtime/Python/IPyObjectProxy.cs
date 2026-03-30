using System.ComponentModel;

namespace CSnakes.Runtime.Python;

/// <summary>
/// A proxy for a <see cref="PyObject"/> that manages its lifetime as well as providing access to
/// it.
/// </summary>
public interface IPyObjectProxy : IDisposable
{
    /// <summary>
    /// Returns a direct reference to the underlying <see cref="PyObject"/> that's reserved for
    /// advanced and internal uses.
    /// </summary>
    /// <remarks>
    /// The caller <em>does not</em> own the returned reference and therefore should not dispose it.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    PyObject DangerousInternalReference { get; }
}

public static class PyObjectProxyExtensions
{
    /// <summary>
    /// Gets a new reference to the underlying <see cref="PyObject"/>. It is the caller's
    /// responsibility to dispose of the reference when done, which can be done explicitly or
    /// implicitly via garbage collection.
    /// </summary>
    public static PyObject GetPyObject(this IPyObjectProxy proxy)
    {
        using (GIL.Acquire())
            return proxy.DangerousInternalReference.Clone();
    }
}
