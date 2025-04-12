using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

/// <summary>
/// This type and its members, although technically public in visibility, are
/// not intended for direct consumption in user code. They are used by the
/// generated code and may be modified or removed in future releases.
/// </summary>
public static partial class PyObjectImporters
{
    public sealed class Clone : IPyObjectImporter<PyObject>
    {
        static PyObject IPyObjectImporter<PyObject>.UnsafeImport(PyObject obj) => obj.Clone();
    }

    public sealed class Runtime<T> : IPyObjectImporter<T>
    {
        static T IPyObjectImporter<T>.UnsafeImport(PyObject obj) => obj.As<T>();
    }

    public sealed class Boolean : IPyObjectImporter<bool>
    {
        static bool IPyObjectImporter<bool>.UnsafeImport(PyObject obj) => CPythonAPI.IsPyTrue(obj);
    }

    public sealed class Int64 : IPyObjectImporter<long>
    {
        static long IPyObjectImporter<long>.UnsafeImport(PyObject obj) => CPythonAPI.PyLong_AsLongLong(obj);
    }

    public sealed class Double : IPyObjectImporter<double>
    {
        static double IPyObjectImporter<double>.UnsafeImport(PyObject obj) => CPythonAPI.PyFloat_AsDouble(obj);
    }

    public sealed class String : IPyObjectImporter<string>
    {
        internal static string Import(PyObject obj)
        {
            using (GIL.Acquire())
                return UnsafeImport(obj);
        }

        static string UnsafeImport(PyObject obj) => CPythonAPI.PyUnicode_AsUTF8(obj);

        static string IPyObjectImporter<string>.UnsafeImport(PyObject obj) => UnsafeImport(obj);
    }

    public sealed class ByteArray : IPyObjectImporter<byte[]>
    {
        static byte[] IPyObjectImporter<byte[]>.UnsafeImport(PyObject obj) => CPythonAPI.PyBytes_AsByteArray(obj);
    }

    public sealed class Buffer : IPyObjectImporter<IPyBuffer>
    {
        static IPyBuffer IPyObjectImporter<IPyBuffer>.UnsafeImport(PyObject obj) =>
            CPythonAPI.IsBuffer(obj)
                ? new PyBuffer(obj)
                : throw InvalidCastException("buffer", obj);
    }

    public sealed class Tuple<T, TImporter> : IPyObjectImporter<ValueTuple<T>>
        where TImporter : IPyObjectImporter<T>
    {
        static ValueTuple<T> IPyObjectImporter<ValueTuple<T>>.UnsafeImport(PyObject obj)
        {
            CheckTuple(obj);
            using var item = GetTupleItem(obj, 0);
            return new(TImporter.UnsafeImport(item));
        }
    }

    public sealed class Sequence<T, TImporter> : IPyObjectImporter<IReadOnlyList<T>>
        where TImporter : IPyObjectImporter<T>
    {
        static IReadOnlyList<T> IPyObjectImporter<IReadOnlyList<T>>.UnsafeImport(PyObject obj) =>
            CPythonAPI.IsPySequence(obj)
                ? new PyList<T, TImporter>(obj.Clone())
                : throw InvalidCastException("sequence", obj);
    }

    public sealed class List<T, TImporter> : IPyObjectImporter<IReadOnlyList<T>>
        where TImporter : IPyObjectImporter<T>
    {
        internal static IReadOnlyList<T> UnsafeImport(PyObject obj) =>
            CPythonAPI.IsPyList(obj)
                ? new PyList<T, TImporter>(obj.Clone())
                : throw InvalidCastException("list", obj);

        static IReadOnlyList<T> IPyObjectImporter<IReadOnlyList<T>>.UnsafeImport(PyObject obj) =>
            UnsafeImport(obj);
    }

    public sealed class Dictionary<TKey, TValue, TKeyImporter, TValueImporter> :
        IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
        where TKeyImporter : IPyObjectImporter<TKey>
        where TValueImporter : IPyObjectImporter<TValue>
    {
        internal static IReadOnlyDictionary<TKey, TValue> Import(PyObject obj)
        {
            using (GIL.Acquire())
                return UnsafeImport(obj);
        }

        static IReadOnlyDictionary<TKey, TValue> UnsafeImport(PyObject obj) =>
            CPythonAPI.IsPyDict(obj)
                ? new PyDictionary<TKey, TValue, TKeyImporter, TValueImporter>(obj.Clone())
                : throw InvalidCastException("dict", obj);

        static IReadOnlyDictionary<TKey, TValue> IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>.UnsafeImport(PyObject obj) =>
            UnsafeImport(obj);
    }

    public sealed class Mapping<TKey, TValue, TKeyImporter, TValueImporter> :
        IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
        where TKeyImporter : IPyObjectImporter<TKey>
        where TValueImporter : IPyObjectImporter<TValue>
    {
        static IReadOnlyDictionary<TKey, TValue> IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>.UnsafeImport(PyObject obj) =>
            CPythonAPI.IsPyMappingWithItems(obj)
                ? new PyDictionary<TKey, TValue, TKeyImporter, TValueImporter>(obj.Clone())
                : throw InvalidCastException("mapping with items", obj);
    }

    public sealed class Generator<TYield, TSend, TReturn, TYieldImporter, TReturnImporter> :
        IPyObjectImporter<IGeneratorIterator<TYield, TSend, TReturn>>
        where TYieldImporter : IPyObjectImporter<TYield>
        where TReturnImporter : IPyObjectImporter<TReturn>
    {
        static IGeneratorIterator<TYield, TSend, TReturn> IPyObjectImporter<IGeneratorIterator<TYield, TSend, TReturn>>.UnsafeImport(PyObject obj) =>
            CPythonAPI.IsPyGenerator(obj)
                ? new GeneratorIterator<TYield, TSend, TReturn, TYieldImporter, TReturnImporter>(obj.Clone())
                : throw InvalidCastException("generator", obj);
    }

    public sealed class Coroutine<TYield, TSend, TReturn, TYieldImporter, TReturnImporter> :
        IPyObjectImporter<ICoroutine<TYield, TSend, TReturn>>
        where TYieldImporter : IPyObjectImporter<TYield>
        where TReturnImporter : IPyObjectImporter<TReturn>
    {
        static ICoroutine<TYield, TSend, TReturn> IPyObjectImporter<ICoroutine<TYield, TSend, TReturn>>.UnsafeImport(PyObject obj) =>
            CPythonAPI.IsPyCoroutine(obj)
                ? new Python.Coroutine<TYield, TSend, TReturn, TYieldImporter, TReturnImporter>(obj.Clone())
                : throw InvalidCastException("coroutine", obj);
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
