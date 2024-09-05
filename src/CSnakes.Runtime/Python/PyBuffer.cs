using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;
internal class PyBuffer : IPyBuffer
{
    private readonly CPythonAPI.Py_buffer _buffer;

    public PyBuffer(PyObject exporter)
    {
        _buffer = CPythonAPI.GetBuffer(exporter);
    }

    public void Dispose()
    {
        CPythonAPI.ReleaseBuffer(_buffer);
    }

    public Int64 Length => _buffer.len;

    public bool Scalar => _buffer.ndim == 0;

    private unsafe void EnsureFormat(char format)
    {
        if (!_buffer.format[0].Equals(format))
        {
            throw new InvalidOperationException($"Buffer is not a {format}, it is {_buffer.format[0]}");
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
        return new Span<Int32>((void*)_buffer.buf, (int)(Length / 4));
    }

    public unsafe Span<UInt32> AsUInt32Scalar()
    {
        EnsureScalar();
        EnsureFormat('L'); // TODO: I is also valid
        return new Span<UInt32>((void*)_buffer.buf, (int)(Length / 4));
    }

    public unsafe Span<Int64> AsInt64Scalar()
    {
        EnsureScalar();
        EnsureFormat('q');
        return new Span<Int64>((void*)_buffer.buf, (int)(Length / 8));
    }

    public unsafe Span<UInt64> AsUInt64Scalar()
    {
        EnsureScalar();
        EnsureFormat('Q');
        return new Span<UInt64>((void*)_buffer.buf, (int)(Length / 8));
    }
}
