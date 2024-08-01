using Python.Runtime;

namespace CSnakes.Runtime.CustomConverters;

public sealed class ListConverter<TKey> : IPyObjectEncoder, IPyObjectDecoder
{
    public bool CanDecode(PyType objectType, Type targetType) =>
        targetType.IsGenericType && typeof(IEnumerable<TKey>).IsAssignableFrom(targetType);

    public bool CanEncode(Type type) =>
        type.IsGenericType && typeof(IEnumerable<TKey>).IsAssignableFrom(type);

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        value = (T)(object)pyObj.AsEnumerable<TKey>().ToList();
        return true;
    }

    public PyObject TryEncode(object value)
    {
        var list = (IEnumerable<TKey>)value;

        var pyList = new PyList();
        foreach (var item in list)
        {
            pyList.Append(item.ToPython());
        }

        return pyList;
    }
}
