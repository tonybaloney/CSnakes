using CommunityToolkit.HighPerformance;
using CSnakes.Runtime.CPython;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif

namespace CSnakes.Runtime.Python;

[NativeMarshalling(typeof(Marshaller))]
public abstract partial class PyBuffer : SafeHandle, IPyBuffer
{
    private protected readonly int ItemCount;

    private CPythonAPI.Py_buffer _buffer;

    internal PyBuffer(in CPythonAPI.Py_buffer buffer, int itemSize) : base(IntPtr.Zero, ownsHandle: true)
    {
        ItemCount = (int)(buffer.len / itemSize);
        _buffer = buffer;
        unsafe { SetHandle((nint)buffer.obj); }
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (IsInvalid)
            return true;

        if (!CPythonAPI.IsInitialized)
        {
            // The Python environment has been disposed, and therefore Python has freed its memory
            // pools. Don't release buffer since the Python process isn't running and this pointer
            // will point somewhere else.
            // TODO: Consider moving this to a logger.
            Debug.WriteLine($"Python buffer for exporter at 0x{handle:X} was released, but Python is no longer running.");
        }
        else if (GIL.IsAcquired)
        {
            // If the GIL is acquired, we can safely release the buffer without acquiring it again
            CPythonAPI.ReleaseBuffer(this);
        }
        else
        {
            // Probably in the GC finalizer thread, instead of causing GIL contention, put this on a
            // queue to be processed later.
            GIL.QueueForDisposal(ref _buffer);
            unsafe { Debug.Assert(_buffer.buf is null); }
        }

        handle = IntPtr.Zero;
        return true;
    }

    private ref readonly CPythonAPI.Py_buffer Buffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ObjectDisposedException.ThrowIf(IsInvalid, this);
            return ref _buffer;
        }
    }

    private protected unsafe T* Pointer<T>() where T : unmanaged => (T*)Buffer.buf;

    public long Length => Buffer.len;

    public bool IsScalar => Buffer.ndim is 0 or 1;

    public bool IsReadOnly => Buffer.@readonly == 1;

    private protected void ThrowIfReadOnly()
    {
        if (IsReadOnly)
            Throw();

        [DoesNotReturn]
        static void Throw() => throw new InvalidOperationException("Buffer is read-only.");
    }

    public int Dimensions => Buffer.ndim switch { 0 => 1, var n => n };

    public abstract Type ItemType { get; }

    private protected unsafe ReadOnlySpan<nint> Shape
    {
        get
        {
            ref readonly var buffer = ref Buffer;
            return new ReadOnlySpan<nint>(buffer.shape, buffer.ndim);
        }
    }

    Span<TItemType> IPyBuffer.AsSpan<TItemType>() =>
        typeof(TItemType) != ItemType
            ? throw new InvalidOperationException($"Cannot cast buffer of type {ItemType} to {typeof(TItemType)}.")
            : ((PyArrayBuffer<TItemType>)this).UnsafeAsSpan();

    ReadOnlySpan<TItemType> IPyBuffer.AsReadOnlySpan<TItemType>() =>
#pragma warning disable CS0618 // Type or member is obsolete
        ((IPyBuffer)this).AsSpan<TItemType>();
#pragma warning restore CS0618 // Type or member is obsolete

    Span2D<TItemType> IPyBuffer.AsSpan2D<TItemType>() =>
        typeof(TItemType) != ItemType
            ? throw new InvalidOperationException($"Cannot cast buffer of type {ItemType} to {typeof(TItemType)}.")
            : ((PyArray2DBuffer<TItemType>)this).UnsafeAsSpan2D();

    ReadOnlySpan2D<TItemType> IPyBuffer.AsReadOnlySpan2D<TItemType>() =>
#pragma warning disable CS0618 // Type or member is obsolete
        ((IPyBuffer)this).AsSpan2D<TItemType>();
#pragma warning restore CS0618 // Type or member is obsolete

#if NET9_0_OR_GREATER
    TensorSpan<TItemType> IPyBuffer.AsTensorSpan<TItemType>() =>
        typeof(TItemType) != ItemType
            ? throw new InvalidOperationException($"Cannot cast buffer of type {ItemType} to {typeof(TItemType)}.")
            : ((PyTensorBuffer<TItemType>)this).UnsafeAsTensorSpan();

    ReadOnlyTensorSpan<TItemType> IPyBuffer.AsReadOnlyTensorSpan<TItemType>() =>
#pragma warning disable CS0618 // Type or member is obsolete
        ((IPyBuffer)this).AsTensorSpan<TItemType>();
#pragma warning restore CS0618 // Type or member is obsolete
#endif // NET9_0_OR_GREATER

    /// <summary>
    /// Struct byte order and offset type see
    /// https://docs.python.org/3/library/struct.html#byte-order-size-and-alignment
    /// </summary>
    private enum ByteOrder
    {
        Native = '@', // default, native byte-order, size and alignment
        Standard = '=', // native byte-order, standard size and no alignment
        Little = '<', // little-endian, standard size and no alignment
        Big = '>', // big-endian, standard size and no alignment
        Network = '!' // big-endian, standard size and no alignment
    }

    struct Unknown;

    internal static IPyBuffer Create(PyObject exporter)
    {
        using (GIL.Acquire())
        {
            return CPythonAPI.IsBuffer(exporter)
                 ? CPythonAPI.GetBuffer(exporter)
                 : throw new ArgumentException("The provided Python object does not support the buffer protocol.", nameof(exporter));
        }
    }

    [CustomMarshaller(typeof(PyBuffer), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
    [CustomMarshaller(typeof(PyBuffer), MarshalMode.ManagedToUnmanagedOut, typeof(ManagedToUnmanagedOut))]
    internal static class Marshaller
    {
        public static class ManagedToUnmanagedIn
        {
            /// <remarks>
            /// Required by the marshaller infrastructure, but never called/used.
            /// </remarks>
            public static nint ConvertToUnmanaged(PyBuffer buffer) =>
                throw new NotSupportedException();

            public static ref readonly CPythonAPI.Py_buffer GetPinnableReference(PyBuffer buffer) =>
                ref buffer.Buffer;
        }

        public static class ManagedToUnmanagedOut
        {
            public static PyBuffer ConvertToManaged(in CPythonAPI.Py_buffer buffer)
            {
                // Assume that the caller never touches the working "buffer" and it is for this
                // implementation to use as it pleases. In case of an error, "Create" will release
                // the buffer before throwing an exception.

                unsafe { return buffer.buf is null ? null! : Create(ref Unsafe.AsRef(in buffer)); }
            }
        }
    }

    /// <remarks>
    /// This is a low-level API and assumes that the GIL is acquired!
    /// </remarks>
    internal static PyBuffer Create(ref CPythonAPI.Py_buffer buffer)
    {
        try
        {
            unsafe
            {
                // The shape and strides fields can never be null because "CPythonAPI.IsBuffer"
                // requests the buffer with the "PyBUF_C_CONTIGUOUS" flag, which includes and
                // implies "PyBUF_STRIDES":
                //
                //     #define PyBUF_C_CONTIGUOUS (0x0020 | PyBUF_STRIDES)
                //
                // Therefore per the buffer protocol, the exporter must fill those even if they may
                // not be used by a particular "PyBuffer<T>" subclass.

                if (buffer is { shape: null } or { strides: null })
                    throw new NotSupportedException("Buffer does not have shape and strides.");
            }

            string format;
            unsafe { format = Utf8StringMarshaller.ConvertToManaged(buffer.format) ?? "B"; }

            if (GetByteOrder(format) is not ByteOrder.Native)
                return new PyBuffer<Unknown>(buffer); // Return a generic buffer if byte order is not native

            var bufferObject = buffer.ndim switch
            {
                0 or 1 => // Treat scalar (ndim: 0) as a 1D array for simplicity
                    GetFormat(format) switch
                    {
                        Format.Half => new PyArrayBuffer<Half>(buffer),
                        Format.Float => new PyArrayBuffer<float>(buffer),
                        Format.Double => new PyArrayBuffer<double>(buffer),
                        Format.Char => new PyArrayBuffer<sbyte>(buffer),
                        Format.UChar => new PyArrayBuffer<byte>(buffer),
                        Format.Short => new PyArrayBuffer<short>(buffer),
                        Format.UShort => new PyArrayBuffer<ushort>(buffer),
                        Format.Int => new PyArrayBuffer<int>(buffer),
                        Format.UInt => new PyArrayBuffer<uint>(buffer),
                        Format.Long when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArrayBuffer<int>(buffer), // LLP64 (long is 32 bits)
                        Format.ULong when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArrayBuffer<uint>(buffer), // LLP64 (long is 32 bits)
                        Format.Long => new PyArrayBuffer<long>(buffer), // LP64 (long is 64 bits)
                        Format.ULong => new PyArrayBuffer<ulong>(buffer), // LP64 (long is 64 bits)
                        Format.LongLong => new PyArrayBuffer<long>(buffer),
                        Format.ULongLong => new PyArrayBuffer<ulong>(buffer),
                        Format.Bool => new PyArrayBuffer<bool>(buffer),
                        Format.SizeT => new PyArrayBuffer<nuint>(buffer),
                        Format.SSizeT => new PyArrayBuffer<nint>(buffer),
                        _ => null,
                    },
                2 =>
                    GetFormat(format) switch
                    {
                        Format.Half => new PyArray2DBuffer<Half>(buffer),
                        Format.Float => new PyArray2DBuffer<float>(buffer),
                        Format.Double => new PyArray2DBuffer<double>(buffer),
                        Format.Char => new PyArray2DBuffer<sbyte>(buffer),
                        Format.UChar => new PyArray2DBuffer<byte>(buffer),
                        Format.Short => new PyArray2DBuffer<short>(buffer),
                        Format.UShort => new PyArray2DBuffer<ushort>(buffer),
                        Format.Int => new PyArray2DBuffer<int>(buffer),
                        Format.UInt => new PyArray2DBuffer<uint>(buffer),
                        Format.Long when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArray2DBuffer<int>(buffer), // LLP64 (long is 32 bits)
                        Format.ULong when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyArray2DBuffer<uint>(buffer), // LLP64 (long is 32 bits)
                        Format.Long => new PyArray2DBuffer<long>(buffer), // LP64 (long is 64 bits)
                        Format.ULong => new PyArray2DBuffer<ulong>(buffer), // LP64 (long is 64 bits)
                        Format.LongLong => new PyArray2DBuffer<long>(buffer),
                        Format.ULongLong => new PyArray2DBuffer<ulong>(buffer),
                        Format.Bool => new PyArray2DBuffer<bool>(buffer),
                        Format.SizeT => new PyArray2DBuffer<nuint>(buffer),
                        Format.SSizeT => new PyArray2DBuffer<nint>(buffer),
                        _ => null,
                    },
#if NET9_0_OR_GREATER
                > 2 =>
                    GetFormat(format) switch
                    {
                        Format.Half => new PyTensorBuffer<Half>(buffer),
                        Format.Float => new PyTensorBuffer<float>(buffer),
                        Format.Double => new PyTensorBuffer<double>(buffer),
                        Format.Char => new PyTensorBuffer<sbyte>(buffer),
                        Format.UChar => new PyTensorBuffer<byte>(buffer),
                        Format.Short => new PyTensorBuffer<short>(buffer),
                        Format.UShort => new PyTensorBuffer<ushort>(buffer),
                        Format.Int => new PyTensorBuffer<int>(buffer),
                        Format.UInt => new PyTensorBuffer<uint>(buffer),
                        Format.Long when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyTensorBuffer<int>(buffer), // LLP64 (long is 32 bits)
                        Format.ULong when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new PyTensorBuffer<uint>(buffer), // LLP64 (long is 32 bits)
                        Format.Long => new PyTensorBuffer<long>(buffer), // LP64 (long is 64 bits)
                        Format.ULong => new PyTensorBuffer<ulong>(buffer), // LP64 (long is 64 bits)
                        Format.LongLong => new PyTensorBuffer<long>(buffer),
                        Format.ULongLong => new PyTensorBuffer<ulong>(buffer),
                        Format.Bool => new PyTensorBuffer<bool>(buffer),
                        Format.SizeT => new PyTensorBuffer<nuint>(buffer),
                        Format.SSizeT => new PyTensorBuffer<nint>(buffer),
                        _ => null,
                    },
#endif // NET9_0_OR_GREATER
                _ => (PyBuffer?)null,
            };

            return bufferObject ?? new PyBuffer<Unknown>(buffer);
        }
        catch
        {
            GIL.Require();
            CPythonAPI.ReleaseBuffer(ref buffer);
            throw;
        }
    }

    private static ByteOrder GetByteOrder(string format) =>
        // The first character of the format string is the byte order
        // If the format string is empty, the byte order is native
        // If the first character is not a byte order, the byte order is native
        format switch
        {
            [var ch, ..] when Enum.IsDefined((ByteOrder)ch) => (ByteOrder)ch,
            _ => ByteOrder.Native
        };

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
        Half = 'e',  // float16
        Float = 'f', // C float
        Double = 'd', // C double
        SizeT = 'n', // C size_t
        SSizeT = 'N', // C ssize_t
    }

    private static Format? GetFormat(string format)
    {
        // The format string contains the type of the buffer, normally in the first
        // position, but the first character can also be the byte order.
        foreach (Format f in format)
        {
            if (Enum.IsDefined(f))
               return f;
        }

        return null;
    }
}

public interface IPyBuffer<T> : IPyBuffer where T : unmanaged;

public class PyBuffer<T> : PyBuffer, IPyBuffer<T> where T : unmanaged
{
    private protected static readonly unsafe int ItemSize = sizeof(T);

    internal PyBuffer(in CPythonAPI.Py_buffer buffer) : base(buffer, ItemSize) { }

    public override Type ItemType => typeof(T);

    private protected unsafe T* Pointer => Pointer<T>();
}
