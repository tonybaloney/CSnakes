using CSnakes.Runtime.CPython;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    private class StockArray<T>(int length)
    {
        private T[]? array;

        public Span<T> GetArray(int minimumLength)
        {
            if (minimumLength == 0)
                return [];

            if (minimumLength > length)
                return new T[minimumLength];

            return (this.array ??= new T[length])[..minimumLength];
        }
    }

    static class ThreadStatics
    {
        [ThreadStatic]
        private static StockArray<nint>? spilledHandles;

        [ThreadStatic]
        private static StockArray<PyObjectMarshaller.ManagedToUnmanagedIn>? spilledMarshallers;

        private const int StockArrayThresholdLength = 100;

        public static StockArray<nint> SpilledHandles => spilledHandles ??= new StockArray<nint>(StockArrayThresholdLength);
        public static StockArray<PyObjectMarshaller.ManagedToUnmanagedIn> SpilledMarshallers => spilledMarshallers ??= new StockArray<PyObjectMarshaller.ManagedToUnmanagedIn>(StockArrayThresholdLength);
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
        var spilledHandles = ThreadStatics.SpilledHandles.GetArray(spillLength);

        var initialMarshallers = new ArrayOf8<PyObjectMarshaller.ManagedToUnmanagedIn>();
        var spilledMarshallers = ThreadStatics.SpilledMarshallers.GetArray(spillLength);

        scoped var uninitializedMarshallers = MemoryMarshal.CreateSpan(ref Unsafe.As<ArrayOf8<PyObjectMarshaller.ManagedToUnmanagedIn>, PyObjectMarshaller.ManagedToUnmanagedIn>(ref initialMarshallers), stackSpillThreshold);
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
