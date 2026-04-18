using CSnakes.Runtime.CPython;
using System.Buffers;

namespace CSnakes.Runtime.Python;

public sealed class PyArrayBuffer<T> : PyBuffer<T> where T : unmanaged
{
    private UnmanagedMemoryManager? _memoryManager;

    internal PyArrayBuffer(in CPythonAPI.Py_buffer buffer) : base(Validate(buffer)) { }

    internal static ref readonly CPythonAPI.Py_buffer Validate(in CPythonAPI.Py_buffer buffer)
    {
        if (buffer.len % ItemSize != 0)
            throw new ArgumentException($"Buffer length is {buffer.len}, which is not a multiple of {ItemSize}.", nameof(buffer));

        if (buffer.itemsize != ItemSize)
            throw new ArgumentException($"Buffer item size is {buffer.itemsize} instead of {ItemSize}.", nameof(buffer));

        return ref buffer;
    }

    public T this[int index]
    {
        get => AsSpan()[index];
        set
        {
            ThrowIfReadOnly();
            AsSpan()[index] = value;
        }
    }

    public TResult Map<TResult>(ReadOnlySpanFunc<T, TResult> function) =>
        function(AsSpan());

    public TResult Map<TArg, TResult>(TArg arg, ReadOnlySpanFunc<T, TArg, TResult> function)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
        => function(AsSpan(), arg);

    public TResult Map<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, ReadOnlySpanFunc<T, TArg1, TArg2, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
        => function(AsSpan(), arg1, arg2);

    public TResult Map<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ReadOnlySpanFunc<T, TArg1, TArg2, TArg3, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
        => function(AsSpan(), arg1, arg2, arg3);

    public void Do(SpanAction<T> action)
    {
        ThrowIfReadOnly();
        action(AsSpan());
    }

    public void Do<TArg>(TArg arg, SpanAction<T, TArg> action)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan(), arg);
    }

    public void Do<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, SpanAction<T, TArg1, TArg2> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan(), arg1, arg2);
    }

    public void Do<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, SpanAction<T, TArg1, TArg2, TArg3> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan(), arg1, arg2, arg3);
    }

    public void CopyFrom(scoped ReadOnlySpan<T> source)
    {
        ThrowIfReadOnly();
        source.CopyTo(AsSpan());
    }

    public void CopyTo(scoped Span<T> destination) => AsSpan().CopyTo(destination);

    // TODO Mark `PyArrayBuffer<T>.AsSpan` private when `IPyBuffer<T>.AsSpan<T>` is removed
    internal unsafe Span<T> AsSpan() => new((void*)Buffer.buf, ItemCount);

    /// <summary>
    /// Gets the memory owner that provides access to the memory directly underlying the buffer,
    /// which is tied to the lifetime of the buffer. <em>Usage after disposing the buffer or the
    /// memory owner will lead to corruption and crashes</em>.
    /// </summary>
    /// <remarks>
    /// When the memory owner is disposed, the buffer will be disposed as well. See <see
    /// href="https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines">
    /// <see cref="Memory{T}"/> and <see cref="Span{T}"/> usage guidelines</see> for more
    /// information.
    /// </remarks>
    public IMemoryOwner<T> UnsafeMemoryOwner => _memoryManager ??= new UnmanagedMemoryManager(this);

    private sealed class UnmanagedMemoryManager(PyArrayBuffer<T> buffer) : MemoryManager<T>
    {
        public override Span<T> GetSpan() => buffer.AsSpan();

        public override unsafe MemoryHandle Pin(int elementIndex = 0)
        {
            // There is no pinning to do since we are dealing with unmanaged memory and the GC
            // will not move or collect it. The memory handle will point directly to the
            // buffer's data at the specified index.

            ArgumentOutOfRangeException.ThrowIfNegative(elementIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(elementIndex, buffer.ItemCount);
            return new MemoryHandle((T*)buffer.Buffer.buf + elementIndex);
        }

        public override void Unpin()
        {
            // Nothing to do here since pinning isn't needed for unmanaged memory.
        }

        protected override void Dispose(bool disposing) => buffer.Dispose();

        protected override bool TryGetArray(out ArraySegment<T> segment)
        {
            // Not supported for unmanaged memory
            segment = default;
            return false;
        }
    }
}
