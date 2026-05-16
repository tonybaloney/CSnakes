using CSnakes.Linq;
using CSnakes.Runtime.CPython;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

/// <summary>
/// This type and its members, although technically public in visibility, are
/// not intended for direct consumption in user code. They are used by the
/// generated code and may be modified or removed in future releases.
/// </summary>
[Experimental("PRTEXP001")]
public static partial class PyObjectImporters
{
    public sealed class Clone : IPyObjectImporter<PyObject>
    {
        private Clone() { }

        static PyObject IPyObjectImporter<PyObject>.BareImport(PyObject obj)
        {
            GIL.Require();
            return obj.Clone();
        }
    }

    public sealed class None : IPyObjectImporter<PyObject>
    {
        private None() { }

        static PyObject IPyObjectImporter<PyObject>.BareImport(PyObject obj)
        {
            GIL.Require();
            return obj.IsNone() ? PyObject.None : throw InvalidCastException("None", obj);
        }
    }

    /// <remarks>
    /// This importer delegates to run-time conversion via <see cref="PyObject.As{T}"/>, which is
    /// annotated with <see cref="RequiresDynamicCodeAttribute"/>. It should be avoided in code that
    /// wishes to support Native AOT scenarios and publishing. Any explicit use of this should be
    /// carefully reviewed and marked with <see cref="RequiresDynamicCodeAttribute"/>.
    /// </remarks>
    public sealed class Runtime<T> : IPyObjectImporter<T>
    {
        private Runtime() { }

        static T IPyObjectImporter<T>.BareImport(PyObject obj)
        {
            GIL.Require();
#pragma warning disable IL3050 // Avoid calling members annotated with 'RequiresDynamicCodeAttribute' when publishing as Native AOT
            return obj.As<T>();
#pragma warning restore IL3050 // Avoid calling members annotated with 'RequiresDynamicCodeAttribute' when publishing as Native AOT
        }
    }

    public sealed class Boolean : IPyObjectImporter<bool>
    {
        private Boolean() { }

        static bool IPyObjectImporter<bool>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPyTrue(obj);
        }
    }

    public sealed class Int64 : IPyObjectImporter<long>
    {
        private Int64() { }

        static long IPyObjectImporter<long>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.PyLong_AsLongLong(obj);
        }
    }

    public sealed class Double : IPyObjectImporter<double>
    {
        private Double() { }

        static double IPyObjectImporter<double>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.PyFloat_AsDouble(obj);
        }
    }

    public sealed class String : IPyObjectImporter<string>
    {
        private String() { }

        static string IPyObjectImporter<string>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.PyUnicode_AsUTF8(obj);
        }
    }

    public sealed class ByteArray : IPyObjectImporter<byte[]>
    {
        private ByteArray() { }

        static byte[] IPyObjectImporter<byte[]>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.PyBytes_AsByteArray(obj);
        }
    }

    public sealed class Buffer : IPyObjectImporter<IPyBuffer>
    {
        private Buffer() { }

        static IPyBuffer IPyObjectImporter<IPyBuffer>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsBuffer(obj)
                ? new PyBuffer(obj)
                : throw InvalidCastException("buffer", obj);
        }
    }

    public sealed class Tuple<T, TImporter> : IPyObjectImporter<ValueTuple<T>>
        where TImporter : IPyObjectImporter<T>
    {
        private Tuple() { }

        static ValueTuple<T> IPyObjectImporter<ValueTuple<T>>.BareImport(PyObject obj)
        {
            GIL.Require();
            CheckTuple(obj);
            using var item = GetTupleItem(obj, 0);
            return new(TImporter.BareImport(item));
        }
    }

    public sealed class VarTuple<T, TImporter> : IPyObjectImporter<ImmutableArray<T>>
        where TImporter : IPyObjectImporter<T>
    {
        private VarTuple() { }

        static ImmutableArray<T> IPyObjectImporter<ImmutableArray<T>>.BareImport(PyObject obj)
        {
            GIL.Require();
            CheckTuple(obj);
            var length = CPythonAPI.PyTuple_Size(obj);
            var builder = ImmutableArray.CreateBuilder<T>(checked((int)length));
            for (int i = 0; i < length; i++)
            {
                using var item = GetTupleItem(obj, i);
                builder.Add(TImporter.BareImport(item));
            }
            return builder.MoveToImmutable();
        }
    }

    public sealed class Sequence<T, TImporter> : IPyObjectImporter<IReadOnlyList<T>>
        where TImporter : IPyObjectImporter<T>
    {
        private Sequence() { }

        static IReadOnlyList<T> IPyObjectImporter<IReadOnlyList<T>>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPySequence(obj)
                ? new PyList<T, TImporter>(obj.Clone())
                : throw InvalidCastException("sequence", obj);
        }
    }

    public sealed class List<T, TImporter> : IPyObjectImporter<IReadOnlyList<T>>
        where TImporter : IPyObjectImporter<T>
    {
        private List() { }

        internal static IReadOnlyList<T> BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPyList(obj)
                ? new PyList<T, TImporter>(obj.Clone())
                : throw InvalidCastException("list", obj);
        }

        static IReadOnlyList<T> IPyObjectImporter<IReadOnlyList<T>>.BareImport(PyObject obj) =>
            BareImport(obj);
    }

    public sealed class Dictionary<TKey, TValue, TKeyImporter, TValueImporter> :
        IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
        where TKeyImporter : IPyObjectImporter<TKey>
        where TValueImporter : IPyObjectImporter<TValue>
    {
        private Dictionary() { }

        static IReadOnlyDictionary<TKey, TValue> IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPyDict(obj)
                ? new PyDictionary<TKey, TValue, TKeyImporter, TValueImporter>(obj.Clone())
                : throw InvalidCastException("dict", obj);
        }
    }

    public sealed class Mapping<TKey, TValue, TKeyImporter, TValueImporter> :
        IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
        where TKeyImporter : IPyObjectImporter<TKey>
        where TValueImporter : IPyObjectImporter<TValue>
    {
        private Mapping() { }

        internal static IReadOnlyDictionary<TKey, TValue> BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPyMappingWithItems(obj)
                ? new PyDictionary<TKey, TValue, TKeyImporter, TValueImporter>(obj.Clone())
                : throw InvalidCastException("mapping with items", obj);
        }

        static IReadOnlyDictionary<TKey, TValue> IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>.BareImport(PyObject obj) =>
            BareImport(obj);
    }

    public sealed class Generator<TYield, TSend, TReturn, TYieldImporter, TReturnImporter> :
        IPyObjectImporter<IGeneratorIterator<TYield, TSend, TReturn>>
        where TYieldImporter : IPyObjectImporter<TYield>
        where TReturnImporter : IPyObjectImporter<TReturn>
    {
        private Generator() { }

        static IGeneratorIterator<TYield, TSend, TReturn> IPyObjectImporter<IGeneratorIterator<TYield, TSend, TReturn>>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPyGenerator(obj)
                ? new GeneratorIterator<TYield, TSend, TReturn, TYieldImporter, TReturnImporter>(obj.Clone())
                : throw InvalidCastException("generator", obj);
        }
    }

    public sealed class Awaitable<T, TImporter> :
        IPyObjectImporter<IAwaitable<T>>
        where TImporter : IPyObjectImporter<T>
    {
        private Awaitable() { }

        static IAwaitable<T> IPyObjectImporter<IAwaitable<T>>.BareImport(PyObject obj)
        {
            GIL.Require();
            return CPythonAPI.IsPyAwaitable(obj)
                ? new Python.Awaitable<T, TImporter>(obj.Clone())
                : throw InvalidCastException("awaitable", obj);
        }
    }

    public sealed class Optional<T, TImporter> : IPyObjectImporter<T?>
        where T : class
        where TImporter : IPyObjectImporter<T>
    {
        private Optional() { }

        static T? IPyObjectImporter<T?>.BareImport(PyObject obj) =>
            !obj.IsNone() ? TImporter.BareImport(obj) : null;
    }

    public sealed class OptionalValue<T, TImporter> : IPyObjectImporter<T?>
        where T : struct
        where TImporter : IPyObjectImporter<T>
    {
        private OptionalValue() { }

        static T? IPyObjectImporter<T?>.BareImport(PyObject obj) =>
            !obj.IsNone() ? TImporter.BareImport(obj) : null;
    }

#pragma warning disable RS0016 // FIXME
    public sealed class Extern<T> : IPyObjectImporter<T>
#pragma warning restore RS0016
        where T : IPyObjectReadable<T>
    {
        static T IPyObjectImporter<T>.BareImport(PyObject obj)
        {
            GIL.Require();
            return T.Reader.Read(obj);
        }
    }

    private static PyObject GetTupleItem(PyObject obj, int index) =>
        PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRef(obj, index));

    private static void CheckTuple(PyObject obj)
    {
        if (CPythonAPI.IsPyTuple(obj))
            return;

        throw InvalidCastException("tuple", obj);
    }

    private static InvalidCastException InvalidCastException(string expected, PyObject actual) =>
        new($"Expected a {expected}, but got {actual.GetPythonType()}");
}
