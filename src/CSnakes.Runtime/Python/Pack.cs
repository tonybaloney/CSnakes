using CSnakes.Runtime.CPython;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PyObjectMarshaller = System.Runtime.InteropServices.Marshalling.SafeHandleMarshaller<CSnakes.Runtime.Python.PyObject>;

namespace CSnakes.Runtime.Python;

/// <summary>
/// These methods are used internally to create a PyObject where the Dispose() call will dispose all items in 
/// the collection inside the same call stack. This avoids the .NET GC Finalizer thread from disposing the items
/// that were created and creating a GIL contention issue when other code is running.
/// </summary>
internal static class Pack
{
    internal static PyObject CreateTuple(Span<PyObject> items) =>
        PyObject.Create(CreateListOrTuple<TupleBuilder>(items));

    internal static PyObject CreateList(Span<PyObject> items) =>
        PyObject.Create(CreateListOrTuple<ListBuilder>(items));

    private interface IListOrTupleBuilder
    {
        static abstract nint New(nint size);
        static abstract int SetItemRaw(nint ob, nint pos, nint o);
    }

    private sealed class ListBuilder : IListOrTupleBuilder
    {
        private ListBuilder() { }

        // As per Python/C API docs for `PyList_New`:
        //
        // > If len is greater than zero, the returned list object's items are set to `NULL`. Thus
        // > you cannot use abstract API functions such as `PySequence_SetItem()` or expose the
        // > object to Python code before setting all items to a real object with
        // > `PyList_SetItem()`.
        //
        // Source: https://docs.python.org/3/c-api/list.html#c.PyList_New

        public static IntPtr New(IntPtr size) => CPythonAPI.PyList_New(size);
        public static int SetItemRaw(IntPtr ob, IntPtr pos, IntPtr o) => CPythonAPI.PyList_SetItemRaw(ob, pos, o);
    }

    private sealed class TupleBuilder : IListOrTupleBuilder
    {
        private TupleBuilder() { }
        public static IntPtr New(IntPtr size) => size == 0 ? CPythonAPI.GetPyEmptyTuple() : CPythonAPI.PyTuple_New(size);
        public static int SetItemRaw(IntPtr ob, IntPtr pos, IntPtr o) => CPythonAPI.PyTuple_SetItemRaw(ob, pos, o);
    }

    const int FixedArrayLength = 8;

    [InlineArray(FixedArrayLength)]
    private struct ArrayOf8<T>
    {
        private T _;
    }

    /// <remarks>
    /// The disposal of the rental is not idempotent!
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    private readonly struct RentedArray<T>(ArrayPool<T> pool, int length) : IDisposable
    {
        private readonly T[] array = pool.Rent(length);

        public int Length => length;
        public Span<T> Span => this.array.AsSpan(0, length);
        public void Dispose() => pool.Return(this.array);

        public Span<T>.Enumerator GetEnumerator() => this.Span.GetEnumerator();

        public static implicit operator Span<T>(RentedArray<T> rented) => rented.Span;

        private string DebuggerDisplay => $"Length = {Length} (Capacity = {this.array.Length})";
    }

    private static class ArrayPools
    {
        private const int MaxLength = 100;
        private const int MaxPerBucket = 10;

        private static readonly ArrayPool<nint> Handles = ArrayPool<nint>.Create(MaxLength, MaxPerBucket);
        private static readonly ArrayPool<PyObjectMarshaller.ManagedToUnmanagedIn> Marshallers = ArrayPool<PyObjectMarshaller.ManagedToUnmanagedIn>.Create(MaxLength, MaxPerBucket);

        public static RentedArray<nint> RentHandles(int length) => new(Handles, length);
        public static RentedArray<PyObjectMarshaller.ManagedToUnmanagedIn> RentMarshallers(int length) => new(Marshallers, length);
    }

    private static nint CreateListOrTuple<TBuilder>(Span<PyObject> items)
        where TBuilder : IListOrTupleBuilder
    {
        // Allocate initial space for the handles and marshallers on the stack.
        // If the number of items exceeds the stack space, allocate and spill
        // the rest into an array on the heap.

        const int stackSpillThreshold = FixedArrayLength;
        var spillLength = Math.Max(0, items.Length - stackSpillThreshold);

        Span<nint> initialHandles = stackalloc nint[Math.Min(stackSpillThreshold, items.Length)];
        using var spilledHandles = ArrayPools.RentHandles(spillLength);

        var initialMarshallers = new ArrayOf8<PyObjectMarshaller.ManagedToUnmanagedIn>();
        using var spilledMarshallers = ArrayPools.RentMarshallers(spillLength);

        var uninitializedMarshallers = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayOf8<PyObjectMarshaller.ManagedToUnmanagedIn>, PyObjectMarshaller.ManagedToUnmanagedIn>(ref initialMarshallers), stackSpillThreshold);
        var uninitializedHandles = initialHandles;

        // The following loop initializes the marshallers and handles for each
        // item in the input span. It is assumed that no exceptions are thrown
        // during this loop. The marshallers are freed in the finally block of
        // the actual list/tuple initialization.

        foreach (var item in items)
        {
            PyObjectMarshaller.ManagedToUnmanagedIn m = default;
            m.FromManaged(item);

            if (uninitializedMarshallers.IsEmpty && spilledMarshallers is { Length: > 0 } sms)
            {
                uninitializedMarshallers = sms;
            }

            uninitializedMarshallers[0] = m;
            uninitializedMarshallers = uninitializedMarshallers[1..];

            if (uninitializedHandles.IsEmpty && spilledHandles is { Length: > 0 } shs)
            {
                uninitializedHandles = shs;
            }

            uninitializedHandles[0] = m.ToUnmanaged();
            uninitializedHandles = uninitializedHandles[1..];
        }

        nint obj = 0;

        try
        {
            obj = TBuilder.New(items.Length);
            SetItems(spilledHandles, SetItems(initialHandles, 0));

            return obj;

            int SetItems(Span<nint> handles, int i)
            {
                foreach (var handle in handles)
                {
                    int result = TBuilder.SetItemRaw(obj, i++, handle);
                    if (result == -1)
                    {
                        throw PyObject.ThrowPythonExceptionAsClrException();
                    }
                }

                return i;
            }
        }
        catch
        {
            if (obj != 0)
            {
                CPythonAPI.Py_DecRefRaw(obj);
            }

            throw;
        }
        finally
        {
            if (spilledMarshallers.Length > 0)
            {
                foreach (var m in initialMarshallers)
                {
                    m.Free();
                }

                foreach (var m in spilledMarshallers)
                {
                    m.Free();
                }
            }
            else
            {
                foreach (var m in initialMarshallers[..items.Length])
                {
                    m.Free();
                }
            }
        }
    }
}
