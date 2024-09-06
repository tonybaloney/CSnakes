using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;
internal sealed class PyBuffer : IPyBuffer, IDisposable
{
    private readonly CPythonAPI.Py_buffer _buffer;
    private bool _disposed;
    private bool _isScalar;
    private string _format;

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
        _buffer = CPythonAPI.GetBuffer(exporter);
        _disposed = false;
        _isScalar = _buffer.ndim == 0 || _buffer.ndim == 1;
        _format = Utf8StringMarshaller.ConvertToManaged(_buffer.format) ?? string.Empty;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CPythonAPI.ReleaseBuffer(_buffer);
            _disposed = true;
        }
    }

    public Int64 Length => _buffer.len;

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

    private unsafe Span<T> AsSpan<T>(Format format) where T : unmanaged
    {
        EnsureScalar();
        EnsureFormat(format);
        return new Span<T>((void*)_buffer.buf, (int)(Length / sizeof(T)));
    }

    public Span<Int32> AsInt32Span() => AsSpan<Int32>(Format.Long); // TODO: i is also valid

    public Span<UInt32> AsUInt32Span() => AsSpan<UInt32>(Format.ULong); // TODO: I is also valid

    public Span<Int64> AsInt64Span() => AsSpan<Int64>(Format.LongLong);

    public  Span<UInt64> AsUInt64Span() => AsSpan<UInt64>(Format.ULongLong);

    public  Span<float> AsFloatSpan() => AsSpan<float>(Format.Float);

    public Span<double> AsDoubleSpan() => AsSpan<double>(Format.Double);

    private unsafe Span2D<T> As2DSpan<T>(Format format) where T : unmanaged
    {
        EnsureFormat(format);
        EnsureDimensions(2);
        EnsureShapeAndStrides();

        return new Span2D<T>(
            (void*) _buffer.buf,
            (int) _buffer.shape[0],
            (int) _buffer.shape[1],
            (int)((int) _buffer.strides[0] - (_buffer.shape[1]* _buffer.itemsize)) // pitch = stride - (width * itemsize)
        );
    }

    public Span2D<int> AsInt32Span2D() => As2DSpan<int>(Format.Long);

    public Span2D<uint> AsUInt32Span2D() => As2DSpan<uint>(Format.ULong);

    public Span2D<long> AsInt64Span2D() => As2DSpan<long>(Format.LongLong);

    public Span2D<ulong> AsUInt64Span2D() => As2DSpan<ulong>(Format.ULongLong);

    public Span2D<float> AsFloatSpan2D() => As2DSpan<float>(Format.Float);

    public Span2D<double> AsDoubleSpan2D() => As2DSpan<double>(Format.Double);

}
