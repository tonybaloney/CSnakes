using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
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
