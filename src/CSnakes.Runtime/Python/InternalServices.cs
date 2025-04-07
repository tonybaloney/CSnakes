using CSnakes.Runtime.CPython;
using System.Numerics;

namespace CSnakes.Runtime.Python;

/// <summary>
/// This type and its members, although technically public in visibility, are
/// not intended for direct consumption in user code. They are used by the
/// generated code and may be modified or removed in future releases.
/// </summary>
public static class InternalServices
{
    public static PyObject Clone(PyObject obj) => obj.Clone();

    public static bool ConvertToBoolean(PyObject obj) =>
        CPythonAPI.IsPyTrue(obj);

    public static int ConvertToInt32(PyObject obj) =>
        (int)CPythonAPI.PyLong_AsLong(obj);

    public static long ConvertToInt64(PyObject obj) =>
        CPythonAPI.PyLong_AsLongLong(obj);

    public static double ConvertToDouble(PyObject obj) =>
        CPythonAPI.PyFloat_AsDouble(obj);

    public static float ConvertToSingle(PyObject obj) =>
        (float)ConvertToDouble(obj);

    public static string ConvertToString(PyObject obj) =>
        CPythonAPI.PyUnicode_AsUTF8(obj);

    public static BigInteger ConvertToBigInteger(PyObject obj) =>
        PyObjectTypeConverter.ConvertToBigInteger(obj, /* unused */ typeof(void));

    public static byte[] ConvertToByteArray(PyObject obj) =>
        CPythonAPI.PyBytes_AsByteArray(obj);

    public static PyObject GetTupleItem(PyObject obj, int index) =>
        PyObject.Create(CPythonAPI.PyTuple_GetItemWithNewRef(obj, index));

    public static void CheckTuple(PyObject obj)
    {
        if (CPythonAPI.IsPyTuple(obj))
            return;

        throw InvalidCastException("tuple", obj);
    }

    public static IReadOnlyList<T> ConvertSequenceToList<T>(PyObject obj,
                                                            Func<PyObject, T>? converter = null) =>
        CPythonAPI.IsPySequence(obj)
            ? new PyList<T>(obj, converter)
            : throw InvalidCastException("sequence", obj);

    public static IReadOnlyList<T> ConvertToList<T>(PyObject obj,
                                                    Func<PyObject, T>? converter = null) =>
        CPythonAPI.IsPyList(obj)
            ? new PyList<T>(obj, converter)
            : throw InvalidCastException("list", obj);

    public static ICoroutine<TYield, TSend, TReturn>
        ConvertToCoroutine<TYield, TSend, TReturn>(PyObject obj,
                                                   Func<PyObject, TYield> converter)
    {
        using (GIL.Acquire()) // TODO Assume this is done by the caller
        {
            return CPythonAPI.IsPyCoroutine(obj)
                 ? new Coroutine<TYield, TSend, TReturn>(obj.Clone(), converter)
                 : throw InvalidCastException("coroutine", obj);
        }
    }

    private static InvalidCastException InvalidCastException(string expected, PyObject actual) =>
        new($"Expected a {expected}, but got {actual.GetPythonType()}");
}
