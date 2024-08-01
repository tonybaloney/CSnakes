using Python.Runtime;
using System.Collections.ObjectModel;

namespace CSnakes.Runtime.CustomConverters;

public sealed class DictionaryConverter<TKey, TValue> : IPyObjectEncoder, IPyObjectDecoder
    where TKey : notnull
{
    public bool CanDecode(PyType objectType, Type targetType) =>
        targetType.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(targetType);

    public bool CanEncode(Type type) =>
        type.IsGenericType && typeof(IReadOnlyDictionary<TKey, TValue>).IsAssignableFrom(type);

    public bool TryDecode<T>(PyObject pyObj, out T value)
    {
        var pyDict = new PyDict(pyObj);

        var items = pyDict.Items();

        var dict = new Dictionary<TKey, TValue>();

        foreach (var item in items)
        {
            var t = item.As<Tuple<TKey, TValue>>();
            dict.Add(t.Item1, t.Item2);
        }

        value = (T)(object)new ReadOnlyDictionary<TKey, TValue>(dict);
        return true;
    }

    public PyObject TryEncode(object value)
    {
        var dict = (IReadOnlyDictionary<TKey, TValue>)value;

        var pyDict = new PyDict();
        foreach (var kvp in dict)
        {
            pyDict.SetItem(kvp.Key.ToPython(), kvp.Value.ToPython());
        }

        return pyDict;
    }
}
