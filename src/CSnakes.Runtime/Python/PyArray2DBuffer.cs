using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

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

    public TResult Map<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, ReadOnlySpan2DFunc<T, TArg1, TArg2, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
        => function(AsSpan2D(), arg1, arg2);

    public TResult Map<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ReadOnlySpan2DFunc<T, TArg1, TArg2, TArg3, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
        => function(AsSpan2D(), arg1, arg2, arg3);

    public void Do(Span2DAction<T> action)
    {
        ThrowIfReadOnly();
        action(AsSpan2D());
    }

    public void Do<TArg>(TArg arg, Span2DAction<T, TArg> action)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan2D(), arg);
    }

    public void Do<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Span2DAction<T, TArg1, TArg2> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan2D(), arg1, arg2);
    }

    public void Do<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, Span2DAction<T, TArg1, TArg2, TArg3> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
    {
        ThrowIfReadOnly();
        action(AsSpan2D(), arg1, arg2, arg3);
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
