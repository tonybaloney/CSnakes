using CSnakes.Runtime.Python;

internal interface IPyObjectImporter<out T>
{
    static abstract T Import(PyObject pyObj);
}

internal sealed class PyObjectImporter<T> :
    IPyObjectImporter<T>
{
    private PyObjectImporter() { }

    public static T Import(PyObject pyObj) => pyObj.As<T>();
}

internal sealed class PyObjectImporter<TKey, TValue> :
    IPyObjectImporter<KeyValuePair<TKey, TValue>>
{
    private PyObjectImporter() { }

    public static KeyValuePair<TKey, TValue> Import(PyObject pyObj) => pyObj.As<TKey, TValue>();
}
