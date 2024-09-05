using CSnakes.Runtime.CPython;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;
internal sealed class PyBuffer : IPyBuffer, IDisposable
{
    private readonly CPythonAPI.Py_buffer _buffer;
    private bool _disposed;
    private bool _isScalar;
    private string _format;

    public unsafe PyBuffer(PyObject exporter)
    {
        _buffer = CPythonAPI.GetBuffer(exporter);
        _disposed = false;
        _isScalar = _buffer.ndim == 0;
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

    private unsafe void EnsureFormat(char format)
    {
        if (!_format.Contains(format))
        {
            throw new InvalidOperationException($"Buffer is not a {format}, it is {_format}");
        }
    }

    private void EnsureScalar()
    {
        if (!Scalar)
        {
            throw new InvalidOperationException("Buffer is not a scalar");
        }
    }

    public unsafe Span<Int32> AsInt32Scalar()
    {
        EnsureScalar();
        EnsureFormat('l'); // TODO: i is also valid
        return new Span<Int32>((void*)_buffer.buf, (int)(Length / sizeof(Int32)));
    }

    public unsafe Span<UInt32> AsUInt32Scalar()
    {
        EnsureScalar();
        EnsureFormat('L'); // TODO: I is also valid
        return new Span<UInt32>((void*)_buffer.buf, (int)(Length / sizeof(UInt32)));
    }

    public unsafe Span<Int64> AsInt64Scalar()
    {
        EnsureScalar();
        EnsureFormat('q');
        return new Span<Int64>((void*)_buffer.buf, (int)(Length / sizeof(Int64)));
    }

    public unsafe Span<UInt64> AsUInt64Scalar()
    {
        EnsureScalar();
        EnsureFormat('Q');
        return new Span<UInt64>((void*)_buffer.buf, (int)(Length / sizeof(UInt64)));
    }

    public unsafe Span<float> AsFloatScalar() {
        EnsureScalar();
        EnsureFormat('f');
        return new Span<float>((void*)_buffer.buf, (int)(Length / sizeof(float)));
    }
}
