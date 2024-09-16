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
                switch (format) {
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
                        return typeof(int);
                    case Format.ULong:
                        return typeof(uint);
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
        return Enum.TryParse(_format[0].ToString(), out ByteOrder byteOrder) ? byteOrder : ByteOrder.Native;
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

    public Span<bool> AsBoolSpan() => AsSpan<bool>(Format.Bool, Format.Bool);
    public Span<byte> AsByteSpan() => AsSpan<byte>(Format.UChar, Format.UChar);
    public Span<sbyte> AsSByteSpan() => AsSpan<sbyte>(Format.Char, Format.Char);

    public Span<short> AsInt16Span() => AsSpan<short>(Format.Short, Format.Short);
    public Span<ushort> AsUInt16Span() => AsSpan<ushort>(Format.UShort, Format.UShort);

    public Span<int> AsInt32Span() => AsSpan<int>(Format.Long, Format.Int);

    public Span<uint> AsUInt32Span() => AsSpan<uint>(Format.ULong, Format.UInt);

    public Span<long> AsInt64Span() => AsSpan<long>(Format.LongLong, Format.Long);

    public  Span<ulong> AsUInt64Span() => AsSpan<ulong>(Format.ULongLong, Format.ULong);

    public  Span<float> AsFloatSpan() => AsSpan<float>(Format.Float, Format.Float);

    public Span<double> AsDoubleSpan() => AsSpan<double>(Format.Double, Format.Double);
    public Span<nint> AsIntPtrSpan() => AsSpan<nint>(Format.SSizeT, Format.SSizeT);
    public Span<nuint> AsUIntPtrSpan() => AsSpan<nuint>(Format.SizeT, Format.SizeT);

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

    public ReadOnlySpan<bool> AsBoolReadOnlySpan() => AsReadOnlySpan<bool>(Format.Bool, Format.Bool);
    public ReadOnlySpan<byte> AsByteReadOnlySpan() => AsReadOnlySpan<byte>(Format.UChar, Format.UChar);
    public ReadOnlySpan<sbyte> AsSByteReadOnlySpan() => AsReadOnlySpan<sbyte>(Format.Char, Format.Char);
    public ReadOnlySpan<short> AsInt16ReadOnlySpan() => AsReadOnlySpan<short>(Format.Short, Format.Short);
    public ReadOnlySpan<ushort> AsUInt16ReadOnlySpan() => AsReadOnlySpan<ushort>(Format.UShort, Format.UShort);
    public ReadOnlySpan<int> AsInt32ReadOnlySpan() => AsReadOnlySpan<int>(Format.Long, Format.Int);
    public ReadOnlySpan<uint> AsUInt32ReadOnlySpan() => AsReadOnlySpan<uint>(Format.ULong, Format.UInt);
    public ReadOnlySpan<long> AsInt64ReadOnlySpan() => AsReadOnlySpan <long>(Format.LongLong, Format.Long);
    public ReadOnlySpan<ulong> AsUInt64ReadOnlySpan() => AsReadOnlySpan<ulong>(Format.ULongLong, Format.ULong);
    public ReadOnlySpan<float> AsFloatReadOnlySpan() => AsReadOnlySpan<float>(Format.Float, Format.Float);
    public ReadOnlySpan<double> AsDoubleReadOnlySpan() => AsReadOnlySpan<double>(Format.Double, Format.Double);
    public ReadOnlySpan<nint> AsIntPtrReadOnlySpan() => AsReadOnlySpan<nint>(Format.SSizeT, Format.SSizeT);
    public ReadOnlySpan<nuint> AsUIntPtrReadOnlySpan() => AsReadOnlySpan<nuint>(Format.SizeT, Format.SizeT);


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

    public Span2D<bool> AsBoolSpan2D() => As2DSpan<bool>(Format.Bool, Format.Bool);
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
    public Span2D<nint> AsIntPtrSpan2D() => As2DSpan<nint>(Format.SSizeT, Format.SSizeT);
    public Span2D<nuint> AsUIntPtrSpan2D() => As2DSpan<nuint>(Format.SizeT, Format.SizeT);

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

    public ReadOnlySpan2D<bool> AsBoolReadOnlySpan2D() => AsReadOnly2DSpan<bool>(Format.Bool, Format.Bool);
    public ReadOnlySpan2D<byte> AsByteReadOnlySpan2D() => AsReadOnly2DSpan<byte>(Format.UChar, Format.UChar);
    public ReadOnlySpan2D<sbyte> AsSByteReadOnlySpan2D() => AsReadOnly2DSpan<sbyte>(Format.Char, Format.Char);
    public ReadOnlySpan2D<short> AsInt16ReadOnlySpan2D() => AsReadOnly2DSpan<short>(Format.Short, Format.Short);
    public ReadOnlySpan2D<ushort> AsUInt16ReadOnlySpan2D() => AsReadOnly2DSpan<ushort>(Format.UShort, Format.UShort);
    public ReadOnlySpan2D<int> AsInt32ReadOnlySpan2D() => AsReadOnly2DSpan<int>(Format.Long, Format.Int);
    public ReadOnlySpan2D<uint> AsUInt32ReadOnlySpan2D() => AsReadOnly2DSpan<uint>(Format.ULong, Format.UInt);
    public ReadOnlySpan2D<long> AsInt64ReadOnlySpan2D() => AsReadOnly2DSpan<long>(Format.LongLong, Format.Long);
    public ReadOnlySpan2D<ulong> AsUInt64ReadOnlySpan2D() => AsReadOnly2DSpan<ulong>(Format.ULongLong, Format.ULong);
    public ReadOnlySpan2D<float> AsFloatReadOnlySpan2D() => AsReadOnly2DSpan<float>(Format.Float, Format.Float);
    public ReadOnlySpan2D<double> AsDoubleReadOnlySpan2D() => AsReadOnly2DSpan<double>(Format.Double, Format.Double);
    public ReadOnlySpan2D<nint> AsIntPtrReadOnlySpan2D() => AsReadOnly2DSpan<nint>(Format.SSizeT, Format.SSizeT);
    public ReadOnlySpan2D<nuint> AsUIntPtrReadOnlySpan2D() => AsReadOnly2DSpan<nuint>(Format.SizeT, Format.SizeT);
}
