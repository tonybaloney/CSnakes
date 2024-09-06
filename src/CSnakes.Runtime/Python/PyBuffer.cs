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
        Native  = '@', // default, native byte-order, size and alignment
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

    public bool Scalar => _isScalar;

    public bool IsReadOnly => _buffer.@readonly == 1;

    public int Dimensions => _buffer.ndim;

    private ByteOrder GetByteOrder()
    {
        // The first character of the format string is the byte order
        // If the format string is empty, the byte order is native
        // If the first character is not a byte order, the byte order is native
        if (_format.Length == 0)
        {
            return ByteOrder.Native;
        }
        return Enum.TryParse(_format[0].ToString(), out ByteOrder byteOrder) ? byteOrder : ByteOrder.Native;
    }

    private void EnsureFormat(char format)
    {
        if (!_format.Contains(format))
        {
            throw new InvalidOperationException($"Buffer is not a {format}, it is {_format}");
        }
    }

    private void EnsureFormat(Format format)
    {
        EnsureFormat((char)format);
    }

    private void EnsureScalar()
    {
        if (!Scalar)
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
        if (_buffer.shape == null || _buffer.strides == null)
        {
            throw new InvalidOperationException("Buffer does not have shape and strides");
        }
    }

    private unsafe Span<T> AsSpan<T>(Format format, Format nixFormat, bool allowReadOnly = false) where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        EnsureScalar();
        // Ensure format for Windows and nixFormat for Linux and macOS
        EnsureFormat(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? format : nixFormat);
        if (allowReadOnly || _buffer.@readonly != 0)
        {
            throw new InvalidOperationException("Buffer is read-only, use an As[T]ReadOnlySpan method.");
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

    public Span<byte> AsByteSpan() => AsSpan<byte>(Format.UChar, Format.UChar);
    public Span<sbyte> AsSByteSpan() => AsSpan<sbyte>(Format.Char, Format.Char);

    public Span<Int16> AsInt16Span() => AsSpan<Int16>(Format.Short, Format.Short);
    public Span<UInt16> AsUInt16Span() => AsSpan<UInt16>(Format.UShort, Format.UShort);

    public Span<Int32> AsInt32Span() => AsSpan<Int32>(Format.Long, Format.Int);

    public Span<UInt32> AsUInt32Span() => AsSpan<UInt32>(Format.ULong, Format.UInt);

    public Span<Int64> AsInt64Span() => AsSpan<Int64>(Format.LongLong, Format.Long);

    public  Span<UInt64> AsUInt64Span() => AsSpan<UInt64>(Format.ULongLong, Format.ULong);

    public  Span<float> AsFloatSpan() => AsSpan<float>(Format.Float, Format.Float);

    public Span<double> AsDoubleSpan() => AsSpan<double>(Format.Double, Format.Double);

    private unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(Format format, Format nixFormat) where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        EnsureScalar();
        // Ensure format for Windows and nixFormat for Linux and macOS
        EnsureFormat(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? format : nixFormat);
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

    public ReadOnlySpan<byte> AsByteReadOnlySpan() => AsReadOnlySpan<byte>(Format.UChar, Format.UChar);
    public ReadOnlySpan<sbyte> AsSByteReadOnlySpan() => AsReadOnlySpan<sbyte>(Format.Char, Format.Char);
    public ReadOnlySpan<Int16> AsInt16ReadOnlySpan() => AsReadOnlySpan<Int16>(Format.Short, Format.Short);
    public ReadOnlySpan<UInt16> AsUInt16ReadOnlySpan() => AsReadOnlySpan<UInt16>(Format.UShort, Format.UShort);
    public ReadOnlySpan<Int32> AsInt32ReadOnlySpan() => AsReadOnlySpan<Int32>(Format.Long, Format.Int);
    public ReadOnlySpan<UInt32> AsUInt32ReadOnlySpan() => AsReadOnlySpan<UInt32>(Format.ULong, Format.UInt);
    public ReadOnlySpan<Int64> AsInt64ReadOnlySpan() => AsReadOnlySpan<Int64>(Format.LongLong, Format.Long);
    public ReadOnlySpan<UInt64> AsUInt64ReadOnlySpan() => AsReadOnlySpan<UInt64>(Format.ULongLong, Format.ULong);
    public ReadOnlySpan<float> AsFloatReadOnlySpan() => AsReadOnlySpan<float>(Format.Float, Format.Float);
    public ReadOnlySpan<double> AsDoubleReadOnlySpan() => AsReadOnlySpan<double>(Format.Double, Format.Double);


    private unsafe Span2D<T> As2DSpan<T>(Format format, Format nixFormat) where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        EnsureFormat(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? format : nixFormat);
        EnsureDimensions(2);
        EnsureShapeAndStrides();
        if (_buffer.@readonly != 0)
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
            (void*) _buffer.buf,
            (int) _buffer.shape[0],
            (int) _buffer.shape[1],
            (int)((int) _buffer.strides[0] - (_buffer.shape[1]* _buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    public Span2D<byte> AsByteSpan2D() => As2DSpan<byte>(Format.UChar, Format.UChar);
    public Span2D<sbyte> AsSByteSpan2D() => As2DSpan<sbyte>(Format.Char, Format.Char);
    public Span2D<short> AsInt16Span2D() => As2DSpan<short>(Format.Short, Format.Short);
    public Span2D<ushort> AsUInt16Span2D() => As2DSpan<ushort>(Format.UShort, Format.UShort);
    public Span2D<int> AsInt32Span2D() => As2DSpan<int>(Format.Long, Format.Int);

    public Span2D<uint> AsUInt32Span2D() => As2DSpan<uint>(Format.ULong, Format.UInt);

    public Span2D<long> AsInt64Span2D() => As2DSpan<long>(Format.LongLong, Format.Long);

    public Span2D<ulong> AsUInt64Span2D() => As2DSpan<ulong>(Format.ULongLong, Format.ULong);

    public Span2D<float> AsFloatSpan2D() => As2DSpan<float>(Format.Float, Format.Float);

    public Span2D<double> AsDoubleSpan2D() => As2DSpan<double>(Format.Double, Format.Double);

    private unsafe ReadOnlySpan2D<T> AsReadOnly2DSpan<T>(Format format, Format nixFormat) where T : unmanaged
    {
        if (_byteOrder != ByteOrder.Native)
        {
            // TODO: support byte order conversion
            throw new InvalidOperationException("Buffer is not in native byte order");
        }
        EnsureFormat(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? format : nixFormat);
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

    public ReadOnlySpan2D<byte> AsByteReadOnlySpan2D() => AsReadOnly2DSpan<byte>(Format.UChar, Format.UChar);
    public ReadOnlySpan2D<sbyte> AsSByteReadOnlySpan2D() => AsReadOnly2DSpan<sbyte>(Format.Char, Format.Char);
    public ReadOnlySpan2D<Int16> AsInt16ReadOnlySpan2D() => AsReadOnly2DSpan<Int16>(Format.Short, Format.Short);
    public ReadOnlySpan2D<UInt16> AsUInt16ReadOnlySpan2D() => AsReadOnly2DSpan<UInt16>(Format.UShort, Format.UShort);
    public ReadOnlySpan2D<int> AsInt32ReadOnlySpan2D() => AsReadOnly2DSpan<int>(Format.Long, Format.Int);
    public ReadOnlySpan2D<uint> AsUInt32ReadOnlySpan2D() => AsReadOnly2DSpan<uint>(Format.ULong, Format.UInt);
    public ReadOnlySpan2D<long> AsInt64ReadOnlySpan2D() => AsReadOnly2DSpan<long>(Format.LongLong, Format.Long);
    public ReadOnlySpan2D<ulong> AsUInt64ReadOnlySpan2D() => AsReadOnly2DSpan<ulong>(Format.ULongLong, Format.ULong);
    public ReadOnlySpan2D<float> AsFloatReadOnlySpan2D() => AsReadOnly2DSpan<float>(Format.Float, Format.Float);
    public ReadOnlySpan2D<double> AsDoubleReadOnlySpan2D() => AsReadOnly2DSpan<double>(Format.Double, Format.Double);
}
