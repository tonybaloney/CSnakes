using Python.Runtime;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PythonEnvironments;

public static class PythonCollection
{
    public static IEnumerable<T> AsEnumerable<T>(this PyObject obj) =>
        new PyIterable(obj.GetIterator()).Select(item => item.As<T>());

    public static IReadOnlyDictionary<TKey, TValue> AsDictionary<TKey, TValue>(this PyObject obj)
    {
        PyDict pd = new(obj);

        Dictionary<TKey, TValue> d = pd.Items()
            .Select(i => i.AsTuple<TKey, TValue>())
            .ToDictionary(i => i.Item1, i => i.Item2);

        return new ReadOnlyDictionary<TKey, TValue>(d);
    }
}
