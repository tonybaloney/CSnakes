using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Internals;

/// <summary>
/// This type and its members, although technically public in visibility, are
/// not intended for direct consumption in user code. They are used by the
/// generated code and may be modified or removed in future releases.
/// </summary>
public interface IPyObjectImporter<out T>
{
    static abstract T Import(PyObject obj);
}

/// <summary>
/// This type and its members, although technically public in visibility, are
/// not intended for direct consumption in user code. They are used by the
/// generated code and may be modified or removed in future releases.
/// </summary>
public static partial class PyObjectImporters
{
    public sealed class Runtime<T> : IPyObjectImporter<T>
    {
        public static T Import(PyObject obj) => obj.As<T>();
    }

    public sealed class Boolean : IPyObjectImporter<bool>
    {
        public static bool Import(PyObject obj) => CPythonAPI.IsPyTrue(obj);
    }

    public sealed class Int64 : IPyObjectImporter<long>
    {
        public static long Import(PyObject obj) => CPythonAPI.PyLong_AsLongLong(obj);
    }

    public sealed class Double : IPyObjectImporter<double>
    {
        public static double Import(PyObject obj) => CPythonAPI.PyFloat_AsDouble(obj);
    }

    public sealed class Single : IPyObjectImporter<float>
    {
        public static float Import(PyObject obj) => (float)CPythonAPI.PyFloat_AsDouble(obj);
    }

    public sealed class String : IPyObjectImporter<string>
    {
        public static string Import(PyObject obj) => CPythonAPI.PyUnicode_AsUTF8(obj);
    }

    public sealed class ByteArray : IPyObjectImporter<byte[]>
    {
        public static byte[] Import(PyObject obj) => CPythonAPI.PyBytes_AsByteArray(obj);
    }

    public sealed class Buffer : IPyObjectImporter<IPyBuffer>
    {
        public static IPyBuffer Import(PyObject obj) =>
            CPythonAPI.IsBuffer(obj)
                ? new PyBuffer(obj)
                : throw InvalidCastException("buffer", obj);
    }

    public sealed class Tuple<T, TImporter> : IPyObjectImporter<ValueTuple<T>>
        where TImporter : IPyObjectImporter<T>
    {
        public static ValueTuple<T> Import(PyObject obj)
        {
            CheckTuple(obj);
            using var item = GetTupleItem(obj, 0);
            return new(TImporter.Import(item));
        }
    }

    public sealed class Sequence<T, TImporter> : IPyObjectImporter<IReadOnlyList<T>>
        where TImporter : IPyObjectImporter<T>
    {
        public static IReadOnlyList<T> Import(PyObject obj) =>
            CPythonAPI.IsPySequence(obj)
                ? new PyList<T, TImporter>(obj.Clone())
                : throw InvalidCastException("sequence", obj);
    }

    public sealed class List<T, TImporter> : IPyObjectImporter<IReadOnlyList<T>>
        where TImporter : IPyObjectImporter<T>
    {
        public static IReadOnlyList<T> Import(PyObject obj) =>
            CPythonAPI.IsPyList(obj)
                ? new PyList<T, TImporter>(obj.Clone())
                : throw InvalidCastException("list", obj);
    }

    public sealed class Dictionary<TKey, TValue, TKeyImporter, TValueImporter> :
        IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
        where TKeyImporter : IPyObjectImporter<TKey>
        where TValueImporter : IPyObjectImporter<TValue>
    {
        public static IReadOnlyDictionary<TKey, TValue> Import(PyObject obj) =>
            CPythonAPI.IsPyDict(obj)
                ? new PyDictionary<TKey, TValue, TKeyImporter, TValueImporter>(obj.Clone())
                : throw InvalidCastException("dict", obj);
    }

    public sealed class Mapping<TKey, TValue, TKeyImporter, TValueImporter> :
        IPyObjectImporter<IReadOnlyDictionary<TKey, TValue>>
        where TKey : notnull
        where TKeyImporter : IPyObjectImporter<TKey>
        where TValueImporter : IPyObjectImporter<TValue>
    {
        public static IReadOnlyDictionary<TKey, TValue> Import(PyObject obj) =>
            CPythonAPI.IsPyMappingWithItems(obj)
                ? new PyDictionary<TKey, TValue, TKeyImporter, TValueImporter>(obj.Clone())
                : throw InvalidCastException("mapping with items", obj);
    }

    public sealed class Coroutine<TYield, TSend, TReturn, TYieldImporter, TSendImporter, TReturnImporter> :
        IPyObjectImporter<ICoroutine<TYield, TSend, TReturn>>
        where TYieldImporter : IPyObjectImporter<TYield>
        where TSendImporter : IPyObjectImporter<TSend>
        where TReturnImporter : IPyObjectImporter<TReturn>
    {
        public static ICoroutine<TYield, TSend, TReturn> Import(PyObject obj)
        {
            using (GIL.Acquire()) // TODO Assume this is done by the caller
            {
                return CPythonAPI.IsPyCoroutine(obj)
                    ? new Python.Coroutine<TYield, TSend, TReturn, TYieldImporter, TSendImporter, TReturnImporter>(obj.Clone())
                    : throw InvalidCastException("coroutine", obj);
            }
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
