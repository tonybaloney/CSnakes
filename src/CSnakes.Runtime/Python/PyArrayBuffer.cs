using CSnakes.Runtime.CPython;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Python;

public sealed class PyArrayBuffer<T> : PyBuffer<T>, IMemoryOwner<T> where T : unmanaged
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
        get => UnsafeAsReadOnlySpan()[index];
        set => UnsafeAsSpan()[index] = value;
    }

    public TResult Map<TResult>(ReadOnlySpanFunc<T, TResult> function) =>
        function(UnsafeAsReadOnlySpan());

    public TResult Map<TArg, TResult>(in TArg arg, ReadOnlySpanFunc<T, TArg, TResult> function)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
        => function(UnsafeAsReadOnlySpan(), arg);

    public TResult Map<TArg1, TArg2, TResult>(in TArg1 arg1, in TArg2 arg2, ReadOnlySpanFunc<T, TArg1, TArg2, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
        => function(UnsafeAsReadOnlySpan(), arg1, arg2);

    public TResult Map<TArg1, TArg2, TArg3, TResult>(in TArg1 arg1, in TArg2 arg2, in TArg3 arg3, ReadOnlySpanFunc<T, TArg1, TArg2, TArg3, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
        => function(UnsafeAsReadOnlySpan(), arg1, arg2, arg3);

    public void Do(SpanAction<T> action) =>
        action(UnsafeAsSpan());

    public void Do<TArg>(in TArg arg, SpanAction<T, TArg> action)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
        => action(UnsafeAsSpan(), arg);

    public void Do<TArg1, TArg2>(in TArg1 arg1, in TArg2 arg2, SpanAction<T, TArg1, TArg2> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
        => action(UnsafeAsSpan(), arg1, arg2);

    public void Do<TArg1, TArg2, TArg3>(in TArg1 arg1, in TArg2 arg2, in TArg3 arg3, SpanAction<T, TArg1, TArg2, TArg3> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
        => action(UnsafeAsSpan(), arg1, arg2, arg3);

    public void CopyFrom(scoped in ReadOnlySpan<T> source) => source.CopyTo(UnsafeAsSpan());
    public void CopyTo(scoped in Span<T> destination) => UnsafeAsReadOnlySpan().CopyTo(destination);

    /// <summary>
    /// Returns a span <em>directly</em> over the buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    public Span<T> UnsafeAsSpan() => UnsafeAsSpan(writeable: true);

    /// <summary>
    /// Returns a read-only span <em>directly</em> over the buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Underlying buffer is read-only.</exception>
    public ReadOnlySpan<T> UnsafeAsReadOnlySpan() => UnsafeAsSpan(writeable: false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe Span<T> UnsafeAsSpan(bool writeable)
    {
        if (writeable)
            ThrowIfReadOnly();
        return new Span<T>(Pointer, ItemCount);
    }

    /// <summary>
    /// Gets the memory directly underlying the buffer, which is tied to the lifetime of the buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    /// <remarks>
    /// See <see
    /// href="https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines"><see
    /// cref="Memory{T}"/> and <see cref="Span{T}"/> usage guidelines</see> for more information.
    /// </remarks>
    public Memory<T> UnsafeMemory => (_memoryManager ??= new UnmanagedMemoryManager(this)).Memory;

    Memory<T> IMemoryOwner<T>.Memory => UnsafeMemory;

    private sealed class UnmanagedMemoryManager(PyArrayBuffer<T> buffer) : MemoryManager<T>
    {
        public override Span<T> GetSpan() => buffer.UnsafeAsSpan();

        public override unsafe MemoryHandle Pin(int elementIndex = 0)
        {
            // There is no pinning to do since we are dealing with unmanaged memory and the GC
            // will not move or collect it. The memory handle will point directly to the
            // buffer's data at the specified index.

            ArgumentOutOfRangeException.ThrowIfNegative(elementIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(elementIndex, buffer.ItemCount);
            return new MemoryHandle(buffer.Pointer + elementIndex);
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
