using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;
internal sealed class PyBuffer : IPyBuffer, IDisposable
{
    private readonly CPythonAPI.Py_buffer _buffer;
    private bool _disposed;
    private readonly bool _isScalar;
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
        Float = 'f', // C float
        Double = 'd', // C double
        SizeT = 'n', // C size_t
        SSizeT = 'N', // C ssize_t
    }

    public unsafe PyBuffer(PyObject exporter)
    {
        using (GIL.Acquire())
        {
            _buffer = CPythonAPI.GetBuffer(exporter);
        }
        _disposed = false;
        _isScalar = _buffer.ndim == 0 || _buffer.ndim == 1;
        _format = Utf8StringMarshaller.ConvertToManaged(_buffer.format) ?? string.Empty;
        _byteOrder = GetByteOrder();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            using (GIL.Acquire())
            {
                CPythonAPI.ReleaseBuffer(_buffer);
            }
            _disposed = true;
        }
    }

    public long Length => _buffer.len;

    public bool IsScalar => _isScalar;

    public bool IsReadOnly => _buffer.@readonly == 1;

    public int Dimensions => _buffer.ndim == 0 ? 1 : _buffer.ndim;

    private unsafe ReadOnlySpan<nint> Shape
    {
        get
        {
            EnsureShapeAndStrides();
            return new ReadOnlySpan<nint>(_buffer.shape, _buffer.ndim);
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
        if (_buffer.shape is null || _buffer.strides is null)
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
        if (_buffer.itemsize != sizeof(T))
        {
            throw new InvalidOperationException($"Buffer item size is {_buffer.itemsize} not {sizeof(T)}");
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
        return new Span<T>((void*)_buffer.buf, (int)(Length / sizeof(T)));
    }

    private unsafe ReadOnlySpan<T> AsReadOnlySpanInternal<T>() where T : unmanaged
    {
        ValidateBufferCommon<T>();
        EnsureScalar();
        return new ReadOnlySpan<T>((void*)_buffer.buf, (int)(Length / sizeof(T)));
    }

    private unsafe Span2D<T> AsSpan2DInternal<T>() where T : unmanaged
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Buffer is read-only, use an As[T]ReadOnlySpan method.");
        }
        ValidateBufferCommon<T>();
        Validate2DBufferCommon<T>();
        return new Span2D<T>(
            (void*)_buffer.buf,
            (int)_buffer.shape[0],
            (int)_buffer.shape[1],
            (int)((int)_buffer.strides[0] - (_buffer.shape[1] * _buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    private unsafe ReadOnlySpan2D<T> AsReadOnlySpan2DInternal<T>() where T : unmanaged
    {
        ValidateBufferCommon<T>();
        Validate2DBufferCommon<T>();
        return new ReadOnlySpan2D<T>(
            (void*)_buffer.buf,
            (int)_buffer.shape[0],
            (int)_buffer.shape[1],
            (int)((int)_buffer.strides[0] - (_buffer.shape[1] * _buffer.itemsize)) // pitch = stride - (width * itemsize)
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
        nint[] strides = new nint[_buffer.ndim];
        for (int i = 0; i < _buffer.ndim; i++)
        {
            strides[i] = _buffer.strides[i] / sizeof(T);
        }
        return new TensorSpan<T>(
            (T*)_buffer.buf,
            _buffer.len,
            Shape,
            strides
        );
    }

    private unsafe ReadOnlyTensorSpan<T> AsReadOnlyTensorSpanInternal<T>() where T : unmanaged
    {
        ValidateBufferCommon<T>();
        EnsureShapeAndStrides();
        nint[] strides = new nint[_buffer.ndim];
        for (int i = 0; i < _buffer.ndim; i++)
        {
            strides[i] = _buffer.strides[i] / sizeof(T);
        }

        return new ReadOnlyTensorSpan<T>(
            (T*)_buffer.buf,
            _buffer.len,
            Shape,
            strides
        );
    }

    public TensorSpan<T> AsTensorSpan<T>() where T : unmanaged => AsTensorSpanInternal<T>();
    public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan<T>() where T : unmanaged => AsReadOnlyTensorSpanInternal<T>();

#endif
    #endregion
}
