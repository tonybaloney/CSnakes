using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;

public interface IPyBuffer<T> : IPyBuffer where T : unmanaged;

public class PyBuffer<T> : IPyBuffer<T> where T : unmanaged
{
    private protected static readonly int ItemSize = Unsafe.SizeOf<T>();

    private protected readonly int ItemCount;

    private CPythonAPI.Py_buffer _buffer;

    internal PyBuffer(in CPythonAPI.Py_buffer buffer)
    {
        ItemCount = (int)(buffer.len / ItemSize);
        _buffer = buffer;
    }

    ~PyBuffer()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private bool IsDisposed => _buffer.buf == 0;

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            using (GIL.Acquire())
                CPythonAPI.ReleaseBuffer(ref _buffer);
        }
        else if (GIL.IsAcquired)
        {
            // If the GIL is acquired, we can safely release the buffer without acquiring it again
            CPythonAPI.ReleaseBuffer(ref _buffer);
        }
        else
        {
            // If the GIL is not acquired, we should not release the buffer here
            // as it may lead to
            GIL.QueueForDisposal(ref _buffer);
            Debug.Assert(_buffer.buf == 0);
            return;
        }

        _buffer = default;
    }

    private protected ref readonly CPythonAPI.Py_buffer Buffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return ref _buffer;
        }
    }

    public long Length => Buffer.len;

    public bool IsScalar => Buffer.ndim is 0 or 1;

    public bool IsReadOnly => Buffer.@readonly == 1;

    private protected void ThrowIfReadOnly()
    {
        if (IsReadOnly)
            Throw();

        [DoesNotReturn]
        static void Throw() => throw new InvalidOperationException("Buffer is read-only.");
    }

    public int Dimensions => Buffer.ndim == 0 ? 1 : Buffer.ndim;

    public Type ItemType => typeof(T);

    private protected unsafe ReadOnlySpan<nint> Shape =>
        _buffer switch
        {
            { shape: not null and var shape, strides: not null, ndim: var ndim } => new ReadOnlySpan<nint>(shape, ndim),
            _ => throw new InvalidOperationException("Buffer does not have shape and strides."),
        };

    Span<TItemType> IPyBuffer.AsSpan<TItemType>() =>
        typeof(TItemType) != typeof(T)
            ? throw new InvalidOperationException($"Cannot cast buffer of type {typeof(T)} to {typeof(TItemType)}.")
            : ((PyArrayBuffer<TItemType>)(IPyBuffer)this).AsSpan();

    ReadOnlySpan<TItemType> IPyBuffer.AsReadOnlySpan<TItemType>() =>
        ((IPyBuffer)this).AsSpan<TItemType>();

    Span2D<TItemType> IPyBuffer.AsSpan2D<TItemType>() =>
        typeof(TItemType) != typeof(T)
            ? throw new InvalidOperationException($"Cannot cast buffer of type {typeof(T)} to {typeof(TItemType)}.")
            : ((PyArray2DBuffer<TItemType>)(IPyBuffer)this).AsSpan2D();

    ReadOnlySpan2D<TItemType> IPyBuffer.AsReadOnlySpan2D<TItemType>() =>
        ((IPyBuffer)this).AsSpan2D<TItemType>();

#if NET9_0_OR_GREATER
    TensorSpan<TItemType> IPyBuffer.AsTensorSpan<TItemType>() =>
        typeof(TItemType) != typeof(T)
            ? throw new InvalidOperationException($"Cannot cast buffer of type {typeof(T)} to {typeof(TItemType)}.")
            : ((PyTensorBuffer<TItemType>)(IPyBuffer)this).AsTensorSpan();

    ReadOnlyTensorSpan<TItemType> IPyBuffer.AsReadOnlyTensorSpan<TItemType>() =>
        ((IPyBuffer)this).AsTensorSpan<TItemType>();
#endif // NET9_0_OR_GREATER
}

public delegate TResult ReadOnlySpanFunc<T, out TResult>(ReadOnlySpan<T> span);

public delegate TResult ReadOnlySpanFunc<T, in TArg, out TResult>(ReadOnlySpan<T> span, TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpanFunc<T, in TArg1, in TArg2, out TResult>(ReadOnlySpan<T> span, TArg1 arg1, TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpanFunc<T, in TArg1, in TArg2, in TArg3, out TResult>(ReadOnlySpan<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

public delegate void SpanAction<T>(Span<T> span);

public delegate void SpanAction<T, in TArg1, in TArg2>(Span<T> span, TArg1 arg1, TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate void SpanAction<T, in TArg1, in TArg2, in TArg3>(Span<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

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

public delegate TResult ReadOnlySpan2DFunc<T, out TResult>(ReadOnlySpan2D<T> span);
public delegate TResult ReadOnlySpan2DFunc<T, in TArg, out TResult>(ReadOnlySpan2D<T> span, TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;
public delegate void Span2DAction<T, in TArg>(Span2D<T> span, TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;
public delegate void Span2DAction<T>(Span2D<T> span);

public sealed class PyArray2DBuffer<T> : PyBuffer<T> where T : unmanaged
{
    internal PyArray2DBuffer(in CPythonAPI.Py_buffer buffer) : base(Validate(buffer)) { }

    private static ref readonly CPythonAPI.Py_buffer Validate(in CPythonAPI.Py_buffer buffer)
    {
        _ = PyArrayBuffer<T>.Validate(buffer);

        if (buffer.ndim != 2)
            throw new ArgumentException("Buffer is not 2D.", nameof(buffer));

        unsafe
        {
            if (buffer is { shape: null } or { strides: null })
                throw new InvalidOperationException("Buffer does not have shape and strides.");

            if (buffer.shape[0] * buffer.shape[1] * ItemSize != buffer.len)
                throw new ArgumentException("Buffer length is not equal to shape.", nameof(buffer));
        }

        return ref buffer;
    }

    public T this[int row, int column]
    {
        get => AsSpan2D()[row, column];
        set
        {
            ThrowIfReadOnly();
            AsSpan2D()[row, column] = value;
        }
    }

    public TResult Map<TResult>(ReadOnlySpan2DFunc<T, TResult> function) =>
        function(AsSpan2D());

    public TResult Map<TArg, TResult>(TArg arg, ReadOnlySpan2DFunc<T, TArg, TResult> function)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
        => function(AsSpan2D(), arg);

    public void Do<TArg>(TArg arg, Span2DAction<T, TArg> action)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan2D(), arg);
    }

    public void Do(Span2DAction<T> action)
    {
        ThrowIfReadOnly();
        action(AsSpan2D());
    }

    public void CopyFrom(scoped ReadOnlySpan2D<T> source)
    {
        ThrowIfReadOnly();
        source.CopyTo(AsSpan2D());
    }

    public void CopyTo(scoped Span<T> destination) => AsSpan2D().CopyTo(destination);
    public void CopyTo(scoped Span2D<T> destination) => AsSpan2D().CopyTo(destination);

    // TODO Mark `PyArray2DBuffer<T>.AsSpan` private when `IPyBuffer<T>.AsSpan2D<T>` is removed
    internal unsafe Span2D<T> AsSpan2D()
    {
        ref readonly var buffer = ref Buffer;
        return new((void*)buffer.buf,
                   height: (int)buffer.shape[0],
                   width: (int)buffer.shape[1],
                   pitch: (int)((int)buffer.strides[0] - (buffer.shape[1] * buffer.itemsize))); // pitch = stride - (width * itemsize)
    }
}

#if NET9_0_OR_GREATER

public delegate TResult ReadOnlyTensorSpanFunc<T, out TResult>(ReadOnlyTensorSpan<T> span);

public delegate TResult ReadOnlyTensorSpanFunc<T, in TArg, out TResult>(ReadOnlyTensorSpan<T> span, TArg arg)
    where TArg : allows ref struct;

public delegate TResult ReadOnlyTensorSpanFunc<T, in TArg1, in TArg2, out TResult>(ReadOnlyTensorSpan<T> span, TArg1 arg1, TArg2 arg2)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct;

public delegate TResult ReadOnlyTensorSpanFunc<T, in TArg1, in TArg2, in TArg3, out TResult>(ReadOnlyTensorSpan<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct;

public delegate void TensorSpanAction<T, in TArg>(TensorSpan<T> span, TArg arg)
    where TArg : allows ref struct;

public delegate void TensorSpanAction<T, in TArg1, in TArg2>(TensorSpan<T> span, TArg1 arg1, TArg2 arg2)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct;

public delegate void TensorSpanAction<T, in TArg1, in TArg2, in TArg3>(TensorSpan<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct;

public delegate void TensorSpanAction<T>(TensorSpan<T> span);

public sealed class PyTensorBuffer<T> : PyBuffer<T> where T : unmanaged
{
    private readonly nint[] strides;
    private nint[]? cachedLengths;

    internal PyTensorBuffer(in CPythonAPI.Py_buffer buffer) : base(Validate(buffer))
    {
        this.strides = new nint[buffer.ndim];
        for (var i = 0; i < buffer.ndim; i++)
        {
            nint stride;
            unsafe
            {
                stride = buffer.strides[i];
            }
            this.strides[i] = stride / ItemSize;
        }
    }

    private static ref readonly CPythonAPI.Py_buffer Validate(in CPythonAPI.Py_buffer buffer)
    {
        _ = PyArrayBuffer<T>.Validate(buffer);

        unsafe
        {
            if (buffer is { shape: null } or { strides: null })
                throw new InvalidOperationException("Buffer does not have shape and strides.");
        }

        return ref buffer;
    }

    public T this[params scoped ReadOnlySpan<nint> indices]
    {
        get => AsTensorSpan()[indices];
        set
        {
            ThrowIfReadOnly();
            AsTensorSpan()[indices] = value;
        }
    }

    public ReadOnlySpan<nint> Lengths
    {
        get
        {
            if (cachedLengths is null)
            {
                var shape = Shape;
                var lengths = new nint[shape.Length];
                shape.CopyTo(lengths);
                cachedLengths = lengths;
            }

            return cachedLengths;
        }
    }

    public TResult Map<TResult>(ReadOnlyTensorSpanFunc<T, TResult> function) =>
        function(AsTensorSpan());

    public TResult Map<TArg, TResult>(TArg arg, ReadOnlyTensorSpanFunc<T, TArg, TResult> function)
        where TArg : allows ref struct =>
        function(AsTensorSpan(), arg);

    public TResult Map<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, ReadOnlyTensorSpanFunc<T, TArg1, TArg2, TResult> function)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct =>
        function(AsTensorSpan(), arg1, arg2);

    public TResult Map<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ReadOnlyTensorSpanFunc<T, TArg1, TArg2, TArg3, TResult> function)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct =>
        function(AsTensorSpan(), arg1, arg2, arg3);

    public void Do<TArg>(TArg arg, TensorSpanAction<T, TArg> action)
        where TArg : allows ref struct
    {
        ThrowIfReadOnly();
        action(AsTensorSpan(), arg);
    }

    public void Do<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, TensorSpanAction<T, TArg1, TArg2> action)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
    {
        ThrowIfReadOnly();
        action(AsTensorSpan(), arg1, arg2);
    }

    public void Do<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TensorSpanAction<T, TArg1, TArg2, TArg3> action)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
    {
        ThrowIfReadOnly();
        action(AsTensorSpan(), arg1, arg2, arg3);
    }

    public void Do(TensorSpanAction<T> action)
    {
        ThrowIfReadOnly();
        action(AsTensorSpan());
    }

    public void CopyFrom(scoped ReadOnlyTensorSpan<T> source)
    {
        ThrowIfReadOnly();
        source.CopyTo(AsTensorSpan());
    }

    public void CopyTo(scoped TensorSpan<T> destination) => AsTensorSpan().CopyTo(destination);

    // TODO Mark `PyTensorBuffer<T>.AsTensorSpan` private when `IPyBuffer<T>.AsTensorSpan<T>` is removed
    internal unsafe TensorSpan<T> AsTensorSpan() => UnsafeAsTensorSpan();

    /// <summary>
    /// Returns a span <em>directly</em> over the tensor buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    public unsafe TensorSpan<T> UnsafeAsTensorSpan()
    {
        ref readonly var buffer = ref Buffer;
        return new((T*)buffer.buf, ItemCount, Shape, this.strides);
    }
}

#endif // NET9_0_OR_GREATER

internal static class PyBuffer
{
    /// <summary>
    /// Struct byte order and offset type see https://docs.python.org/3/library/struct.html#byte-order-size-and-alignment
    /// </summary>
    private enum ByteOrder
    {
        Native = '@', // default, native byte-order, size and alignment
        Standard = '=', // native byte-order, standard size and no alignment
        Little = '<', // little-endian, standard size and no alignment
        Big = '>', // big-endian, standard size and no alignment
        Network = '!' // big-endian, standard size and no alignment
    }

    struct Unknown;

    public static IPyBuffer Create(PyObject exporter)
    {
        CPythonAPI.Py_buffer buffer;
        using (GIL.Acquire())
        {
            if (!CPythonAPI.IsBuffer(exporter))
            {
                throw new ArgumentException("The provided Python object does not support the buffer protocol.",
                    nameof(exporter));
            }

            CPythonAPI.GetBuffer(exporter, out buffer);
        }

        var format = Marshal.PtrToStringUTF8(buffer.format) ?? "B";

        if (GetByteOrder(format) is not ByteOrder.Native)
            return new PyBuffer<Unknown>(buffer); // Return a generic buffer if byte order is not native

        var bufferObject = buffer switch
        {
            { ndim: 0 or 1 } => // Treat scalar (ndim: 0) as a 1D array for simplicity
                GetFormat(format) switch
                {
                    Format.Half => new PyArrayBuffer<Half>(buffer),
                    Format.Float => new PyArrayBuffer<float>(buffer),
                    Format.Double => new PyArrayBuffer<double>(buffer),
                    Format.Char => new PyArrayBuffer<sbyte>(buffer),
                    Format.UChar => new PyArrayBuffer<byte>(buffer),
                    Format.Short => new PyArrayBuffer<short>(buffer),
                    Format.UShort => new PyArrayBuffer<ushort>(buffer),
                    Format.Int => new PyArrayBuffer<int>(buffer),
                    Format.UInt => new PyArrayBuffer<uint>(buffer),
                    Format.Long when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArrayBuffer<int>(buffer), // LLP64 (long is 32 bits)
                    Format.ULong when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArrayBuffer<uint>(buffer), // LLP64 (long is 32 bits)
                    Format.Long => new PyArrayBuffer<long>(buffer), // LP64 (long is 64 bits)
                    Format.ULong => new PyArrayBuffer<ulong>(buffer), // LP64 (long is 64 bits)
                    Format.LongLong => new PyArrayBuffer<long>(buffer),
                    Format.ULongLong => new PyArrayBuffer<ulong>(buffer),
                    Format.Bool => new PyArrayBuffer<bool>(buffer),
                    Format.SizeT => new PyArrayBuffer<nuint>(buffer),
                    Format.SSizeT => new PyArrayBuffer<nint>(buffer),
                    _ => null,
                },
            { ndim: 2, HasShape: true, HasStrides: true } =>
                GetFormat(format) switch
                {
                    Format.Half => new PyArray2DBuffer<Half>(buffer),
                    Format.Float => new PyArray2DBuffer<float>(buffer),
                    Format.Double => new PyArray2DBuffer<double>(buffer),
                    Format.Char => new PyArray2DBuffer<sbyte>(buffer),
                    Format.UChar => new PyArray2DBuffer<byte>(buffer),
                    Format.Short => new PyArray2DBuffer<short>(buffer),
                    Format.UShort => new PyArray2DBuffer<ushort>(buffer),
                    Format.Int => new PyArray2DBuffer<int>(buffer),
                    Format.UInt => new PyArray2DBuffer<uint>(buffer),
                    Format.Long when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArray2DBuffer<int>(buffer), // LLP64 (long is 32 bits)
                    Format.ULong when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArray2DBuffer<uint>(buffer), // LLP64 (long is 32 bits)
                    Format.Long => new PyArray2DBuffer<long>(buffer), // LP64 (long is 64 bits)
                    Format.ULong => new PyArray2DBuffer<ulong>(buffer), // LP64 (long is 64 bits)
                    Format.LongLong => new PyArray2DBuffer<long>(buffer),
                    Format.ULongLong => new PyArray2DBuffer<ulong>(buffer),
                    Format.Bool => new PyArray2DBuffer<bool>(buffer),
                    Format.SizeT => new PyArray2DBuffer<nuint>(buffer),
                    Format.SSizeT => new PyArray2DBuffer<nint>(buffer),
                    _ => null,
                },
#if NET9_0_OR_GREATER
            { ndim: > 2, HasShape: true, HasStrides: true } =>
                GetFormat(format) switch
                {
                    Format.Half => new PyTensorBuffer<Half>(buffer),
                    Format.Float => new PyTensorBuffer<float>(buffer),
                    Format.Double => new PyTensorBuffer<double>(buffer),
                    Format.Char => new PyTensorBuffer<sbyte>(buffer),
                    Format.UChar => new PyTensorBuffer<byte>(buffer),
                    Format.Short => new PyTensorBuffer<short>(buffer),
                    Format.UShort => new PyTensorBuffer<ushort>(buffer),
                    Format.Int => new PyTensorBuffer<int>(buffer),
                    Format.UInt => new PyTensorBuffer<uint>(buffer),
                    Format.Long when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyTensorBuffer<int>(buffer),
                    Format.ULong when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyTensorBuffer<uint>(buffer),
                    Format.Long => new PyTensorBuffer<long>(buffer),
                    Format.ULong => new PyTensorBuffer<ulong>(buffer),
                    Format.LongLong => new PyTensorBuffer<long>(buffer),
                    Format.ULongLong => new PyTensorBuffer<ulong>(buffer),
                    Format.Bool => new PyTensorBuffer<bool>(buffer),
                    Format.SizeT => new PyTensorBuffer<nuint>(buffer),
                    Format.SSizeT => new PyTensorBuffer<nint>(buffer),
                    _ => null,
                },
#endif // NET9_0_OR_GREATER
            _ => (IPyBuffer?)null,
        };

        return bufferObject ?? new PyBuffer<Unknown>(buffer);
    }

    private static ByteOrder GetByteOrder(string format) =>
        // The first character of the format string is the byte order
        // If the format string is empty, the byte order is native
        // If the first character is not a byte order, the byte order is native
        format switch
        {
            [var ch, ..] when Enum.IsDefined((ByteOrder)ch) => (ByteOrder)ch,
            _ => ByteOrder.Native
        };

    private enum Format
    {
        Padding = 'x',
        Char = 'b', // C char
        UChar = 'B', // C unsigned char
        Bool = '?', // C _Bool
        Short = 'h', // C short
        UShort = 'H', // C unsigned short
        Int = 'i', // C int
        UInt = 'I', // C unsigned int
        Long = 'l', // C long
        ULong = 'L', // C unsigned long
        LongLong = 'q', // C long long
        ULongLong = 'Q', // C unsigned long long
        Half = 'e',  // float16
        Float = 'f', // C float
        Double = 'd', // C double
        SizeT = 'n', // C size_t
        SSizeT = 'N', // C ssize_t
    }

    private static Format? GetFormat(string format)
    {
        // The format string contains the type of the buffer, normally in the first
        // position, but the first character can also be the byte order.
        foreach (Format f in format)
        {
            if (Enum.IsDefined(f))
               return f;
        }

        return null;
    }
}
