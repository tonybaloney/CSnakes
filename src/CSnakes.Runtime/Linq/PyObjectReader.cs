#pragma warning disable PRTEXP001
#pragma warning disable RS0016

using CSnakes.Runtime.Python;

namespace CSnakes.Linq;

public interface IPyObjectReader<out T>
{
    T Read(PyObject obj);
}

public interface IPyObjectReadable<out TSelf>
    where TSelf : IPyObjectReadable<TSelf>
{
    static abstract IPyObjectReader<TSelf> Reader { get; }
}

public static class PyObjectReader
{
    public static T To<T>(this PyObject obj, IPyObjectReader<T> reader)
    {
        using (obj)
            return reader.Read(obj);
    }

    public static IPyObjectReader<bool> HasAttr(string name) =>
        Create(obj => obj.HasAttr(name));

    public static IPyObjectReader<PyObject> GetAttr(string name) =>
        Create(obj => obj.GetAttr(name));

    public static IPyObjectReader<T> GetAttr<T>(string name, IPyObjectReader<T> reader) =>
        from value in GetAttr(name)
        select reader.Read(value);

    public static IPyObjectReader<T> GetAttr<T>(string name, IPyObjectReader<T> reader, IPyObjectReader<T> otherwise) =>
        from has in HasAttr(name)
        from value in has ? GetAttr(name, reader) : otherwise
        select value;

    public static IPyObjectReader<T?> GetAttrOrNull<T>(string name, IPyObjectReader<T> reader)
        where T : class =>
        GetAttr(name, reader, Return<T?>(null));

    public static IPyObjectReader<T?> GetAttrOrDefault<T>(string name, IPyObjectReader<T> reader) =>
        from has in HasAttr(name)
        from value in has ? GetAttr(name, reader) : Return(default(T?))
        select value;

    public static IPyObjectReader<T> Call<T>(string name, IPyObjectReader<T> reader) =>
        Create(obj => obj.GetAttr(name).Call().To(reader));

    public static readonly IPyObjectReader<PyObject> Clone = As<PyObject, PyObjectImporters.Clone>();
    public static readonly IPyObjectReader<bool> Boolean = As<bool, PyObjectImporters.Boolean>();
    public static readonly IPyObjectReader<long> Int64 = As<long, PyObjectImporters.Int64>();
    public static readonly IPyObjectReader<double> Double = As<double, PyObjectImporters.Double>();
    public static readonly IPyObjectReader<string> String = As<string, PyObjectImporters.String>();
    public static readonly IPyObjectReader<byte[]> ByteArray = As<byte[], PyObjectImporters.ByteArray>();
    public static readonly IPyObjectReader<IPyBuffer> Buffer = As<IPyBuffer, PyObjectImporters.Buffer>();

    public static IPyObjectReader<TResult> List<T, TResult>(IPyObjectReader<T> reader,
                                                            Func<IEnumerable<T>, TResult> resultSelector)
        where TResult : IList<T> =>
        from seq in GetIter(reader)
        select resultSelector(seq);

    public static IPyObjectReader<TResult> Dict<T, TResult>(IPyObjectReader<T> reader,
                                                            Func<IEnumerable<KeyValuePair<string, T>>, TResult> resultSelector)
        where TResult : IDictionary<string, T> =>
        from dict in As<IReadOnlyDictionary<string, PyObject>, PyObjectImporters.Dictionary<String, PyObject, PyObjectImporters.String, PyObjectImporters.Clone>>()
        select resultSelector(from e in dict
                              select new KeyValuePair<string, T>(e.Key, e.Value.To(reader)));

    public static IPyObjectReader<(T1, T2)> Tuple<T1, T2>(IPyObjectReader<T1> a, IPyObjectReader<T2> b) =>
        from tuple in As<(PyObject, PyObject), PyObjectImporters.Tuple<PyObject, PyObject, PyObjectImporters.Clone, PyObjectImporters.Clone>>()
        select (tuple.Item1.To(a), tuple.Item2.To(b));

    private static IPyObjectReader<T> As<T, TImporter>()
        where TImporter : IPyObjectImporter<T> =>
        Create(static obj => obj.ImportAs<T, TImporter>());

    private static IPyObjectReader<T> As<T, TImporter>(this IPyObjectReader<PyObject> reader)
        where TImporter : IPyObjectImporter<T> =>
        from obj in reader
        select obj.ImportAs<T, TImporter>();

    public static IPyObjectReader<IEnumerable<T>> GetIter<T>(IPyObjectReader<T> selector) =>
        Create(obj => from item in obj.AsEnumerable<PyObject, PyObjectImporters.Clone>()
                      select item.To(selector));

    public static IPyObjectReader<TResult>
        Select<T, TResult>(this IPyObjectReader<T> reader, Func<T, TResult> selector) =>
        Create(obj => selector(reader.Read(obj)));

    public static IPyObjectReader<(T1 First, T2 Second)>
        Zip<T1, T2>(this IPyObjectReader<T1> first, IPyObjectReader<T2> second) =>
        Create(obj => (first.Read(obj), second.Read(obj)));

    public static IPyObjectReader<TResult>
        Bind<T, TResult>(this IPyObjectReader<T> reader, Func<T, IPyObjectReader<TResult>> resultSelector) =>
        Create(obj => resultSelector(reader.Read(obj)).Read(obj));

    public static IPyObjectReader<TResult>
        SelectMany<TFirst, TSecond, TResult>(this IPyObjectReader<TFirst> first,
                                             Func<TFirst, IPyObjectReader<TSecond>> secondSelector,
                                             Func<TFirst, TSecond, TResult> resultSelector) =>
        // Could also be written as:
        // first.Bind(a => secondSelector(a).Bind(b => Return(resultSelector(a, b))));
        Create(obj =>
        {
            var a = first.Read(obj);
            var b = secondSelector(a).Read(obj);
            return resultSelector(a, b);
        });

    public static IPyObjectReader<T> Return<T>(T value) => Create(_ => value);

    public static IPyObjectReader<T> Create<T>(Func<PyObject, T> function) =>
        new DelegatingReader<T>(function);

    private sealed class DelegatingReader<T>(Func<PyObject, T> delegatee) :
        IPyObjectReader<T>
    {
        public T Read(PyObject obj) => delegatee(obj);
    }
}
