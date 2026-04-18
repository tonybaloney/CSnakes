#if NET9_0_OR_GREATER

using System.Numerics.Tensors;
using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public sealed class PyTensorBuffer<T> : PyBuffer<T> where T : unmanaged
{
    private readonly nint[] strides;
    private nint[]? cachedLengths;

    internal PyTensorBuffer(in CPythonAPI.Py_buffer buffer) : base(Validate(buffer))
    {
        this.strides = new nint[buffer.ndim];
        for (var i = 0; i < buffer.ndim; i++)
        {
            nint stride;
            unsafe
            {
                stride = buffer.strides[i];
            }
            this.strides[i] = stride / ItemSize;
        }
    }

    private static ref readonly CPythonAPI.Py_buffer Validate(in CPythonAPI.Py_buffer buffer)
    {
        _ = PyArrayBuffer<T>.Validate(buffer);

        unsafe
        {
            if (buffer is { shape: null } or { strides: null })
                throw new InvalidOperationException("Buffer does not have shape and strides.");
        }

        return ref buffer;
    }

    public T this[params scoped ReadOnlySpan<nint> indices]
    {
        get => AsTensorSpan()[indices];
        set
        {
            ThrowIfReadOnly();
            AsTensorSpan()[indices] = value;
        }
    }

    public ReadOnlySpan<nint> Lengths
    {
        get
        {
            if (cachedLengths is null)
            {
                var shape = Shape;
                var lengths = new nint[shape.Length];
                shape.CopyTo(lengths);
                cachedLengths = lengths;
            }

            return cachedLengths;
        }
    }

    public TResult Map<TResult>(ReadOnlyTensorSpanFunc<T, TResult> function) =>
        function(AsTensorSpan());

    public TResult Map<TArg, TResult>(TArg arg, ReadOnlyTensorSpanFunc<T, TArg, TResult> function)
        where TArg : allows ref struct =>
        function(AsTensorSpan(), arg);

    public TResult Map<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2, ReadOnlyTensorSpanFunc<T, TArg1, TArg2, TResult> function)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct =>
        function(AsTensorSpan(), arg1, arg2);

    public TResult Map<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ReadOnlyTensorSpanFunc<T, TArg1, TArg2, TArg3, TResult> function)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct =>
        function(AsTensorSpan(), arg1, arg2, arg3);

    public void Do<TArg>(TArg arg, TensorSpanAction<T, TArg> action)
        where TArg : allows ref struct
    {
        ThrowIfReadOnly();
        action(AsTensorSpan(), arg);
    }

    public void Do<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, TensorSpanAction<T, TArg1, TArg2> action)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
    {
        ThrowIfReadOnly();
        action(AsTensorSpan(), arg1, arg2);
    }

    public void Do<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TensorSpanAction<T, TArg1, TArg2, TArg3> action)
        where TArg1 : allows ref struct
        where TArg2 : allows ref struct
        where TArg3 : allows ref struct
    {
        ThrowIfReadOnly();
        action(AsTensorSpan(), arg1, arg2, arg3);
    }

    public void Do(TensorSpanAction<T> action)
    {
        ThrowIfReadOnly();
        action(AsTensorSpan());
    }

    public void CopyFrom(scoped ReadOnlyTensorSpan<T> source)
    {
        ThrowIfReadOnly();
        source.CopyTo(AsTensorSpan());
    }

    public void CopyTo(scoped TensorSpan<T> destination) => AsTensorSpan().CopyTo(destination);

    // TODO Mark `PyTensorBuffer<T>.AsTensorSpan` private when `IPyBuffer<T>.AsTensorSpan<T>` is removed
    internal unsafe TensorSpan<T> AsTensorSpan() => UnsafeAsTensorSpan();

    /// <summary>
    /// Returns a span <em>directly</em> over the tensor buffer.
    /// <em>Usage after disposing the buffer will lead to corruption and crashes</em>.
    /// </summary>
    public unsafe TensorSpan<T> UnsafeAsTensorSpan()
    {
        ref readonly var buffer = ref Buffer;
        return new((T*)buffer.buf, ItemCount, Shape, this.strides);
    }
}

#endif // NET9_0_OR_GREATER
