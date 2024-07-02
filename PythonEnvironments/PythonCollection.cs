using Python.Runtime;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PythonEnvironments;

public static class PythonCollection
{
    public static IEnumerable<T> AsEnumerable<T>(this PyObject obj) =>
        new PyIterable(obj.GetIterator()).Select(item => item.As<T>());

    public static IReadOnlyDictionary<TKey, TValue> AsDictionary<TKey, TValue>(this PyObject obj) =>
        new ReadOnlyDictionary<TKey, TValue>(new PyDict(obj).ToDictionary(
            item => item.As<TKey>(),
            item => item.As<TValue>()));
}
