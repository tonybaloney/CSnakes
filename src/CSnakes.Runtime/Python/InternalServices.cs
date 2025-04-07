using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

/// <summary>
/// This type and its members, although technically public in visibility, are
/// not intended for direct consumption in user code. They are used by the
/// generated code and may be modified or removed in future releases.
/// </summary>
public static partial class InternalServices
{
    public interface IConverter<out T>
    {
        static abstract T Convert(PyObject obj);
    }

    public static partial class Converters
    {
        public sealed class Runtime<T> : IConverter<T>
        {
            public static T Convert(PyObject obj) => obj.As<T>();
        }

        public sealed class Boolean : IConverter<bool>
        {
            public static bool Convert(PyObject obj) => CPythonAPI.IsPyTrue(obj);
        }

        public sealed class Int64 : IConverter<long>
        {
            public static long Convert(PyObject obj) => CPythonAPI.PyLong_AsLongLong(obj);
        }

        public sealed class Double : IConverter<double>
        {
            public static double Convert(PyObject obj) => CPythonAPI.PyFloat_AsDouble(obj);
        }

        public sealed class Single : IConverter<float>
        {
            public static float Convert(PyObject obj) => (float)CPythonAPI.PyFloat_AsDouble(obj);
        }

        public sealed class String : IConverter<string>
        {
            public static string Convert(PyObject obj) => CPythonAPI.PyUnicode_AsUTF8(obj);
        }

        public sealed class ByteArray : IConverter<byte[]>
        {
            public static byte[] Convert(PyObject obj) => CPythonAPI.PyBytes_AsByteArray(obj);
        }

        public sealed class Tuple<T, TConverter> : IConverter<ValueTuple<T>>
            where TConverter : IConverter<T>
        {
            public static ValueTuple<T> Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var item = GetTupleItem(obj, 0);
                return new(TConverter.Convert(item));
            }
        }

        public sealed class Sequence<T, TConverter> : IConverter<IReadOnlyList<T>>
            where TConverter : IConverter<T>
        {
            public static IReadOnlyList<T> Convert(PyObject obj) =>
                CPythonAPI.IsPySequence(obj)
                    ? new PyList<T, TConverter>(obj.Clone())
                    : throw InvalidCastException("sequence", obj);
        }

        public sealed class List<T, TConverter> : IConverter<IReadOnlyList<T>>
            where TConverter : IConverter<T>
        {
            public static IReadOnlyList<T> Convert(PyObject obj) =>
                CPythonAPI.IsPyList(obj)
                    ? new PyList<T, TConverter>(obj.Clone())
                    : throw InvalidCastException("list", obj);
        }

        public sealed class Coroutine<TYield, TSend, TReturn, TYieldConverter, TSendConverter, TReturnConverter> :
            IConverter<ICoroutine<TYield, TSend, TReturn>>
            where TYieldConverter : IConverter<TYield>
            where TSendConverter : IConverter<TSend>
            where TReturnConverter : IConverter<TReturn>
        {
            public static ICoroutine<TYield, TSend, TReturn> Convert(PyObject obj)
            {
                using (GIL.Acquire()) // TODO Assume this is done by the caller
                {
                    return CPythonAPI.IsPyCoroutine(obj)
                        ? new Python.Coroutine<TYield, TSend, TReturn, TYieldConverter, TSendConverter, TReturnConverter>(obj.Clone())
                        : throw InvalidCastException("coroutine", obj);
                }
            }
        }
    }

    public static PyObject GetTupleItem(PyObject obj, int index) =>
        PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRef(obj, index));

    public static void CheckTuple(PyObject obj)
    {
        if (CPythonAPI.IsPyTuple(obj))
            return;

        throw InvalidCastException("tuple", obj);
    }

    private static InvalidCastException InvalidCastException(string expected, PyObject actual) =>
        new($"Expected a {expected}, but got {actual.GetPythonType()}");
}
