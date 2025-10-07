using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;
internal sealed class PyBuffer : IPyBuffer
{
    private CPythonAPI.Py_buffer _buffer;
    private readonly string _format;
    private readonly ByteOrder _byteOrder;

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

    public PyBuffer(PyObject exporter)
    {
        using (GIL.Acquire())
        {
            CPythonAPI.GetBuffer(exporter, out _buffer);
        }
        IsScalar = _buffer.ndim is 0 or 1;
        _format = Marshal.PtrToStringUTF8(_buffer.format) ?? string.Empty;
        _byteOrder = GetByteOrder();
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

    private ref readonly CPythonAPI.Py_buffer Buffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return ref _buffer;
        }
    }

    public long Length => Buffer.len;

    public bool IsScalar { get; }

    public bool IsReadOnly => Buffer.@readonly == 1;

    public int Dimensions => Buffer.ndim switch { 0 => 1, var n => n };

    private unsafe ReadOnlySpan<nint> Shape
    {
        get
        {
            EnsureShapeAndStrides();
            return new ReadOnlySpan<nint>(Buffer.shape, Buffer.ndim);
        }
    }

    public Type GetItemType()
    {
        // The format string contains the type of the buffer, normally in the first
        // position, but the first character can also be the byte order.
        foreach (char f in _format)
        {
            if (!Enum.IsDefined((Format)f))
            {
                continue;
            }

            return (Format)f switch
            {
                Format.Half => typeof(Half),
                Format.Float => typeof(float),
                Format.Double => typeof(double),
                Format.Char => typeof(sbyte),
                Format.UChar => typeof(byte),
                Format.Short => typeof(short),
                Format.UShort => typeof(ushort),
                Format.Int => typeof(int),
                Format.UInt => typeof(uint),
                Format.Long => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? typeof(int) : typeof(long),
                Format.ULong => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? typeof(uint) : typeof(ulong),
                Format.LongLong => typeof(long),
                Format.ULongLong => typeof(ulong),
                Format.Bool => typeof(bool),
                Format.SizeT => typeof(nuint),
                Format.SSizeT => typeof(nint),
                _ => throw new InvalidOperationException($"Format {f} not mapped to CLR type")
            };
        }
        throw new InvalidOperationException($"Unknown format {_format}");
    }

    public Span<T> AsSpan<T>() where T : unmanaged => AsSpanInternal<T>();
    public ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged => AsReadOnlySpanInternal<T>();
    public Span2D<T> AsSpan2D<T>() where T : unmanaged => AsSpan2DInternal<T>();
    public ReadOnlySpan2D<T> AsReadOnlySpan2D<T>() where T : unmanaged => AsReadOnlySpan2DInternal<T>();

    private ByteOrder GetByteOrder()
    {
        // The first character of the format string is the byte order
        // If the format string is empty, the byte order is native
        // If the first character is not a byte order, the byte order is native
        if (_format.Length == 0)
        {
            return ByteOrder.Native;
        }

        return Enum.IsDefined(typeof(ByteOrder), (int)_format[0]) ? (ByteOrder)_format[0] : ByteOrder.Native;
    }

    private void EnsureFormat(char format)
    {
        if (!_format.Contains(format))
        {
            throw new InvalidOperationException($"Buffer is not a {format}, it is {_format}");
        }
    }

    private void EnsureScalar()
    {
        if (!IsScalar)
        {
            throw new InvalidOperationException("Buffer is not a scalar");
        }
    }

    private void EnsureDimensions(int dimensions)
    {
        if (Dimensions != dimensions)
        {
            throw new InvalidOperationException($"Buffer is not {dimensions}D");
        }
    }

    private unsafe void EnsureShapeAndStrides()
    {
        if (Buffer.shape is null || Buffer.strides is null)
        {
            throw new InvalidOperationException("Buffer does not have shape and strides");
        }
    }
    private unsafe void ValidateBufferCommon<T>() where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        if (typeof(T) != GetItemType())
        {
            throw new InvalidOperationException($"Buffer item type is {GetItemType()} not {typeof(T)}");
        }
        if (Length % sizeof(T) != 0)
        {
            throw new InvalidOperationException($"Buffer length is not a multiple of {sizeof(T)}");
        }
        if (Buffer.itemsize is var itemSize && itemSize != sizeof(T))
        {
            throw new InvalidOperationException($"Buffer item size is {itemSize} not {sizeof(T)}");
        }
    }

    private unsafe void Validate2DBufferCommon<T>() where T : unmanaged
    {
        EnsureDimensions(2);
        EnsureShapeAndStrides();
        if (Shape[0] * Shape[1] * sizeof(T) != Length)
        {
            throw new InvalidOperationException("Buffer length is not equal to shape");
        }
    }

    private unsafe Span<T> AsSpanInternal<T>() where T : unmanaged
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Buffer is read-only, use the AsReadOnlySpan method.");
        }
        ValidateBufferCommon<T>();
        EnsureScalar();
        return new Span<T>((void*)Buffer.buf, (int)(Length / sizeof(T)));
    }

    private unsafe ReadOnlySpan<T> AsReadOnlySpanInternal<T>() where T : unmanaged
    {
        ValidateBufferCommon<T>();
        EnsureScalar();
        return new ReadOnlySpan<T>((void*)Buffer.buf, (int)(Length / sizeof(T)));
    }

    private unsafe Span2D<T> AsSpan2DInternal<T>() where T : unmanaged
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Buffer is read-only, use an As[T]ReadOnlySpan method.");
        }
        ValidateBufferCommon<T>();
        Validate2DBufferCommon<T>();
        var buffer = Buffer;
        return new Span2D<T>(
            (void*)buffer.buf,
            (int)buffer.shape[0],
            (int)buffer.shape[1],
            (int)((int)buffer.strides[0] - (buffer.shape[1] * buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    private unsafe ReadOnlySpan2D<T> AsReadOnlySpan2DInternal<T>() where T : unmanaged
    {
        ValidateBufferCommon<T>();
        Validate2DBufferCommon<T>();
        var buffer = Buffer;
        return new ReadOnlySpan2D<T>(
            (void*)buffer.buf,
            (int)buffer.shape[0],
            (int)buffer.shape[1],
            (int)((int)buffer.strides[0] - (buffer.shape[1] * buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    #region Tensors
#if NET9_0_OR_GREATER
    private unsafe TensorSpan<T> AsTensorSpanInternal<T>() where T : unmanaged
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Buffer is read-only, use an As[T]ReadOnlyTensorSpan method.");
        }
        ValidateBufferCommon<T>();
        EnsureShapeAndStrides();
        var buffer = Buffer;
        nint[] strides = new nint[buffer.ndim];
        for (int i = 0; i < buffer.ndim; i++)
        {
            strides[i] = buffer.strides[i] / sizeof(T);
        }
        return new TensorSpan<T>(
            (T*)buffer.buf,
            buffer.len / sizeof(T),
            Shape,
            strides
        );
    }

    private unsafe ReadOnlyTensorSpan<T> AsReadOnlyTensorSpanInternal<T>() where T : unmanaged
    {
        ValidateBufferCommon<T>();
        EnsureShapeAndStrides();
        var buffer = Buffer;
        nint[] strides = new nint[buffer.ndim];
        for (int i = 0; i < buffer.ndim; i++)
        {
            strides[i] = buffer.strides[i] / sizeof(T);
        }
        return new ReadOnlyTensorSpan<T>(
            (T*)buffer.buf,
            buffer.len / sizeof(T),
            Shape,
            strides
        );
    }

    public TensorSpan<T> AsTensorSpan<T>() where T : unmanaged => AsTensorSpanInternal<T>();
    public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>() where T : unmanaged => AsReadOnlyTensorSpanInternal<T>();

#endif
    #endregion
}
