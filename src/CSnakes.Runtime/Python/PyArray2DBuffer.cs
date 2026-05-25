using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime.Python;

public sealed class PyArray2DBuffer<T> : PyBuffer<T> where T : unmanaged
{
    readonly int height;
    readonly int width;
    readonly int pitch;

    internal PyArray2DBuffer(in CPythonAPI.Py_buffer buffer) :
        base(Validate(buffer, out var height, out var width, out var pitch))
    {
        this.height = height;
        this.width = width;
        this.pitch = pitch;
    }

    private static ref readonly CPythonAPI.Py_buffer Validate(in CPythonAPI.Py_buffer buffer,
                                                              out int height, out int width, out int pitch)
    {
        _ = PyArrayBuffer<T>.Validate(buffer);

        if (buffer.ndim != 2)
            throw new ArgumentException("Buffer is not 2D.", nameof(buffer));

        unsafe
        {
            if (buffer is { shape: null } or { strides: null })
                throw new InvalidOperationException("Buffer does not have shape and strides.");

            height = (int)buffer.shape[0];
            width = (int)buffer.shape[1];

            if (height * width * ItemSize != buffer.len)
                throw new ArgumentException("Buffer length is not equal to shape.", nameof(buffer));

            pitch = (int)(buffer.strides[0] - (width * buffer.itemsize)); // pitch = stride - (width * itemsize)
        }

        return ref buffer;
    }

    public T this[int row, int column]
    {
        get => UnsafeAsReadOnlySpan2D()[row, column];
        set => UnsafeAsSpan2D()[row, column] = value;
    }

    public TResult Map<TResult>(ReadOnlySpan2DFunc<T, TResult> function) =>
        function(UnsafeAsReadOnlySpan2D());

    public TResult Map<TArg, TResult>(in TArg arg, ReadOnlySpan2DFunc<T, TArg, TResult> function)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
        => function(UnsafeAsReadOnlySpan2D(), arg);

    public TResult Map<TArg1, TArg2, TResult>(in TArg1 arg1, in TArg2 arg2, ReadOnlySpan2DFunc<T, TArg1, TArg2, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
        => function(UnsafeAsReadOnlySpan2D(), arg1, arg2);

    public TResult Map<TArg1, TArg2, TArg3, TResult>(in TArg1 arg1, in TArg2 arg2, in TArg3 arg3, ReadOnlySpan2DFunc<T, TArg1, TArg2, TArg3, TResult> function)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
        => function(UnsafeAsReadOnlySpan2D(), arg1, arg2, arg3);

    public void Do(Span2DAction<T> action) =>
        action(UnsafeAsSpan2D());

    public void Do<TArg>(TArg arg, Span2DAction<T, TArg> action)
#if NET9_0_OR_GREATER
        where TArg : allows ref struct
#endif
        => action(UnsafeAsSpan2D(), arg);

    public void Do<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Span2DAction<T, TArg1, TArg2> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
#endif
        => action(UnsafeAsSpan2D(), arg1, arg2);

    public void Do<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, Span2DAction<T, TArg1, TArg2, TArg3> action)
#if NET9_0_OR_GREATER
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
#endif
        => action(UnsafeAsSpan2D(), arg1, arg2, arg3);

    public void CopyFrom(scoped in ReadOnlySpan2D<T> source) => source.CopyTo(UnsafeAsSpan2D());

    public void CopyTo(scoped in Span<T> destination) => UnsafeAsReadOnlySpan2D().CopyTo(destination);
    public void CopyTo(scoped in Span2D<T> destination) => UnsafeAsReadOnlySpan2D().CopyTo(destination);

    /// <summary>
    /// Returns a span <em>directly</em> over the buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    public Span2D<T> UnsafeAsSpan2D() => UnsafeAsSpan2D(writeable: true);

    /// <summary>
    /// Returns a read-only span <em>directly</em> over the buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Underlying buffer is read-only.</exception>
    public ReadOnlySpan2D<T> UnsafeAsReadOnlySpan2D() => UnsafeAsSpan2D(writeable: false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe Span2D<T> UnsafeAsSpan2D(bool writeable)
    {
        if (writeable)
            ThrowIfReadOnly();
        return new Span2D<T>(Pointer, height: this.height, width: this.width, pitch: this.pitch);
    }
}
