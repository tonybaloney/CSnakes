#pragma warning disable PRTEXP001
#pragma warning disable RS0016

using CSnakes.Runtime.Python;

namespace CSnakes.Linq;

public interface IPyObjectQuery<out T>
{
    T GetResult(PyObject obj);
}

public interface IPyObjectQueryable<out TSelf>
    where TSelf : IPyObjectQueryable<TSelf>
{
    static abstract IPyObjectQuery<TSelf> Query { get; }
}

public static class PyObjectQuery
{
    public static T To<T>(this PyObject obj, IPyObjectQuery<T> query)
    {
        using (obj)
            return query.GetResult(obj);
    }

    public static readonly IPyObjectQuery<PyObject> Id = Create(static obj => obj);

    public static IPyObjectQuery<bool> HasAttr(string name) =>
        Create(obj => obj.HasAttr(name));

    public static IPyObjectQuery<PyObject> GetAttr(string name) =>
        Create(obj => obj.GetAttr(name));

    public static IPyObjectQuery<T> GetAttr<T>(string name, IPyObjectQuery<T> query) =>
        from value in GetAttr(name)
        select query.GetResult(value);

    public static IPyObjectQuery<T> GetAttr<T>(string name, IPyObjectQuery<T> query, IPyObjectQuery<T> otherwise) =>
        from has in HasAttr(name)
        from value in has ? GetAttr(name, query) : otherwise
        select value;

    public static IPyObjectQuery<T?> GetAttrOrNull<T>(string name, IPyObjectQuery<T> query)
        where T : class =>
        GetAttr(name, query, Return<T?>(null));

    public static IPyObjectQuery<T?> GetAttrOrDefault<T>(string name, IPyObjectQuery<T> query) =>
        from has in HasAttr(name)
        from value in has ? GetAttr(name, query) : Return(default(T?))
        select value;

    public static readonly IPyObjectQuery<PyObject> Clone = As<PyObject, PyObjectImporters.Clone>();
    public static readonly IPyObjectQuery<bool> Boolean = As<bool, PyObjectImporters.Boolean>();
    public static readonly IPyObjectQuery<long> Int64 = As<long, PyObjectImporters.Int64>();
    public static readonly IPyObjectQuery<double> Double = As<double, PyObjectImporters.Double>();
    public static readonly IPyObjectQuery<string> String = As<string, PyObjectImporters.String>();
    public static readonly IPyObjectQuery<byte[]> ByteArray = As<byte[], PyObjectImporters.ByteArray>();
    public static readonly IPyObjectQuery<IPyBuffer> Buffer = As<IPyBuffer, PyObjectImporters.Buffer>();

    public static IPyObjectQuery<TResult> List<T, TResult>(IPyObjectQuery<T> query,
                                                           Func<IEnumerable<T>, TResult> resultSelector)
        where TResult : IList<T> =>
        from seq in GetIter(query)
        select resultSelector(seq);

    public static IPyObjectQuery<TResult> Dict<T, TResult>(IPyObjectQuery<T> query,
                                                           Func<IEnumerable<KeyValuePair<string, T>>, TResult> resultSelector)
        where TResult : IDictionary<string, T> =>
        from dict in As<IReadOnlyDictionary<string, PyObject>, PyObjectImporters.Dictionary<String, PyObject, PyObjectImporters.String, PyObjectImporters.Clone>>()
        select resultSelector(from e in dict
                              select new KeyValuePair<string, T>(e.Key, e.Value.To(query)));

    public static IPyObjectQuery<(T1, T2)> Tuple<T1, T2>(IPyObjectQuery<T1> a, IPyObjectQuery<T2> b) =>
        from tuple in As<(PyObject, PyObject), PyObjectImporters.Tuple<PyObject, PyObject, PyObjectImporters.Clone, PyObjectImporters.Clone>>()
        select (tuple.Item1.To(a), tuple.Item2.To(b));

    public static IPyObjectQuery<T> As<T, TImporter>()
        where TImporter : IPyObjectImporter<T> =>
        Create(static obj => obj.ImportAs<T, TImporter>());

    public static IPyObjectQuery<T> As<T, TImporter>(this IPyObjectQuery<PyObject> query)
        where TImporter : IPyObjectImporter<T> =>
        from obj in query
        select obj.ImportAs<T, TImporter>();

    public static IPyObjectQuery<IEnumerable<T>> GetIter<T>(IPyObjectQuery<T> selector) =>
        Create(obj => from item in obj.AsEnumerable<PyObject, PyObjectImporters.Clone>()
                      select item.To(selector));

    public static IPyObjectQuery<TResult>
        Select<T, TResult>(this IPyObjectQuery<T> query, Func<T, TResult> selector) =>
        Create(obj => selector(query.GetResult(obj)));

    public static IPyObjectQuery<(T1 First, T2 Second)>
        Zip<T1, T2>(this IPyObjectQuery<T1> first, IPyObjectQuery<T2> second) =>
        Create(obj => (first.GetResult(obj), second.GetResult(obj)));

    public static IPyObjectQuery<TResult>
        SelectMany<T, TResult>(this IPyObjectQuery<T> first,
                               Func<T, IPyObjectQuery<TResult>> resultSelector) =>
        Create(obj => resultSelector(first.GetResult(obj)).GetResult(obj));

    public static IPyObjectQuery<TResult>
        SelectMany<TFirst, TSecond, TResult>(this IPyObjectQuery<TFirst> first,
                                             Func<TFirst, IPyObjectQuery<TSecond>> secondSelector,
                                             Func<TFirst, TSecond, TResult> resultSelector) =>
        Create(obj =>
        {
            var a = first.GetResult(obj);
            return resultSelector(a, secondSelector(a).GetResult(obj));
        });

    public static IPyObjectQuery<T> Return<T>(T value) => Create(_ => value);

    public static IPyObjectQuery<T> Create<T>(Func<PyObject, T> selector) =>
        new DelegatingQuery<T>(selector);

    private sealed class DelegatingQuery<T>(Func<PyObject, T> selector) :
        IPyObjectQuery<T>
    {
        public T GetResult(PyObject obj) => selector(obj);
    }
}
