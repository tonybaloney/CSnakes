using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

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

    public Type GetItemType()
    {
        // The format string contains the type of the buffer, normally in the first
        // position, but the first character can also be the byte order.
        for (int i = 0; i < _format.Length; i++)
        {
            if (Enum.IsDefined(typeof(Format), (int)_format[i]))
            {
                var format = (Format)_format[i];
                switch (format)
                {
                    case Format.Bool:
                        return typeof(bool);
                    case Format.Char:
                        return typeof(sbyte);
                    case Format.UChar:
                        return typeof(byte);
                    case Format.Short:
                        return typeof(short);
                    case Format.UShort:
                        return typeof(ushort);
                    case Format.Int:
                        return typeof(int);
                    case Format.UInt:
                        return typeof(uint);
                    case Format.Long:
                        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? typeof(int) : typeof(long);
                    case Format.ULong:
                        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? typeof(uint) : typeof(ulong);
                    case Format.LongLong:
                        return typeof(long);
                    case Format.ULongLong:
                        return typeof(ulong);
                    case Format.Float:
                        return typeof(float);
                    case Format.Double:
                        return typeof(double);
                    case Format.SizeT:
                        return typeof(nuint);
                    case Format.SSizeT:
                        return typeof(nint);
                }
            }
        }
        throw new InvalidOperationException($"Unknown format {_format}");
    }

    private ByteOrder GetByteOrder()
    {
        // The first character of the format string is the byte order
        // If the format string is empty, the byte order is native
        // If the first character is not a byte order, the byte order is native
        if (_format.Length == 0)
        {
            return ByteOrder.Native;
        }

        if (Enum.IsDefined(typeof(ByteOrder), (int)_format[0]))
        {
            return (ByteOrder)_format[0];
        }
        else
        {
            return ByteOrder.Native;
        }
    }

    private void EnsureFormat(char format)
    {
        if (!_format.Contains(format))
        {
            throw new InvalidOperationException($"Buffer is not a {format}, it is {_format}");
        }
    }

    private void EnsureFormat(Format format) => EnsureFormat((char)format);

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

    private unsafe Span<T> AsSpanInternal<T>() where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        EnsureScalar();
        if (typeof(T) != GetItemType())
        {
            throw new InvalidOperationException($"Buffer item type is {GetItemType()} not {typeof(T)}");
        }
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Buffer is read-only, use the AsReadOnlySpan method.");
        }
        if (Length % sizeof(T) != 0)
        {
            throw new InvalidOperationException($"Buffer length is not a multiple of {sizeof(T)}");
        }
        if (_buffer.itemsize != sizeof(T))
        {
            throw new InvalidOperationException($"Buffer item size is {_buffer.itemsize} not {sizeof(T)}");
        }
        return new Span<T>((void*)_buffer.buf, (int)(Length / sizeof(T)));
    }

    public Span<T> AsSpan<T>() where T : unmanaged => AsSpanInternal<T>();

    private unsafe ReadOnlySpan<T> AsReadOnlySpanInternal<T>() where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        EnsureScalar();
        // Ensure format for Windows and nixFormat for Linux and macOS
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
        return new ReadOnlySpan<T>((void*)_buffer.buf, (int)(Length / sizeof(T)));
    }

    public ReadOnlySpan<T> AsReadOnlySpan<T>() where T : unmanaged => AsReadOnlySpanInternal<T>();

    private unsafe Span2D<T> AsSpan2DInternal<T>() where T : unmanaged
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
        EnsureDimensions(2);
        EnsureShapeAndStrides();
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Buffer is read-only, use an As[T]ReadOnlySpan method.");
        }
        if (_buffer.shape[0] * _buffer.shape[1] * sizeof(T) != Length)
        {
            throw new InvalidOperationException("Buffer length is not equal to shape");
        }
        if (_buffer.itemsize != sizeof(T))
        {
            throw new InvalidOperationException($"Buffer item size is {_buffer.itemsize} not {sizeof(T)}");
        }
        return new Span2D<T>(
            (void*)_buffer.buf,
            (int)_buffer.shape[0],
            (int)_buffer.shape[1],
            (int)((int)_buffer.strides[0] - (_buffer.shape[1] * _buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    public Span2D<T> AsSpan2D<T>() where T : unmanaged => AsSpan2DInternal<T>();


    private unsafe ReadOnlySpan2D<T> AsReadOnlySpan2DInternal<T>() where T : unmanaged
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
        EnsureDimensions(2);
        EnsureShapeAndStrides();
        if (_buffer.shape[0] * _buffer.shape[1] * sizeof(T) != Length)
        {
            throw new InvalidOperationException("Buffer length is not equal to shape");
        }
        if (_buffer.itemsize != sizeof(T))
        {
            throw new InvalidOperationException($"Buffer item size is {_buffer.itemsize} not {sizeof(T)}");
        }
        return new ReadOnlySpan2D<T>(
            (void*)_buffer.buf,
            (int)_buffer.shape[0],
            (int)_buffer.shape[1],
            (int)((int)_buffer.strides[0] - (_buffer.shape[1] * _buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    public ReadOnlySpan2D<T> AsReadOnlySpan2D<T>() where T : unmanaged => AsReadOnlySpan2DInternal<T>();
}
