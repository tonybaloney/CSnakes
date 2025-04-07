namespace CSnakes.Runtime.Python.Internals;
partial class PyObjectImporters
{
    public sealed class Tuple<T1, T2, TImporter1, TImporter2> :
        IPyObjectImporter<(T1, T2)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
    {
        public static (T1, T2)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            return (TImporter1.Import(a), TImporter2.Import(b));
        }
    }

    public sealed class Tuple<T1, T2, T3, TImporter1, TImporter2, TImporter3> :
        IPyObjectImporter<(T1, T2, T3)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
    {
        public static (T1, T2, T3)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, TImporter1, TImporter2, TImporter3, TImporter4> :
        IPyObjectImporter<(T1, T2, T3, T4)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
    {
        public static (T1, T2, T3, T4)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5> :
        IPyObjectImporter<(T1, T2, T3, T4, T5)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
    {
        public static (T1, T2, T3, T4, T5)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
    {
        public static (T1, T2, T3, T4, T5, T6)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
    {
        public static (T1, T2, T3, T4, T5, T6, T7)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7, TImporter8> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7, T8)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
        where TImporter8 : IPyObjectImporter<T8>
    {
        public static (T1, T2, T3, T4, T5, T6, T7, T8)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            using var h = GetTupleItem(obj, 7);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g), TImporter8.Import(h));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7, TImporter8, TImporter9> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
        where TImporter8 : IPyObjectImporter<T8>
        where TImporter9 : IPyObjectImporter<T9>
    {
        public static (T1, T2, T3, T4, T5, T6, T7, T8, T9)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            using var h = GetTupleItem(obj, 7);
            using var i = GetTupleItem(obj, 8);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g), TImporter8.Import(h), TImporter9.Import(i));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7, TImporter8, TImporter9, TImporter10> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
        where TImporter8 : IPyObjectImporter<T8>
        where TImporter9 : IPyObjectImporter<T9>
        where TImporter10 : IPyObjectImporter<T10>
    {
        public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            using var h = GetTupleItem(obj, 7);
            using var i = GetTupleItem(obj, 8);
            using var j = GetTupleItem(obj, 9);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g), TImporter8.Import(h), TImporter9.Import(i), TImporter10.Import(j));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7, TImporter8, TImporter9, TImporter10, TImporter11> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
        where TImporter8 : IPyObjectImporter<T8>
        where TImporter9 : IPyObjectImporter<T9>
        where TImporter10 : IPyObjectImporter<T10>
        where TImporter11 : IPyObjectImporter<T11>
    {
        public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            using var h = GetTupleItem(obj, 7);
            using var i = GetTupleItem(obj, 8);
            using var j = GetTupleItem(obj, 9);
            using var k = GetTupleItem(obj, 10);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g), TImporter8.Import(h), TImporter9.Import(i), TImporter10.Import(j), TImporter11.Import(k));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7, TImporter8, TImporter9, TImporter10, TImporter11, TImporter12> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
        where TImporter8 : IPyObjectImporter<T8>
        where TImporter9 : IPyObjectImporter<T9>
        where TImporter10 : IPyObjectImporter<T10>
        where TImporter11 : IPyObjectImporter<T11>
        where TImporter12 : IPyObjectImporter<T12>
    {
        public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            using var h = GetTupleItem(obj, 7);
            using var i = GetTupleItem(obj, 8);
            using var j = GetTupleItem(obj, 9);
            using var k = GetTupleItem(obj, 10);
            using var l = GetTupleItem(obj, 11);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g), TImporter8.Import(h), TImporter9.Import(i), TImporter10.Import(j), TImporter11.Import(k), TImporter12.Import(l));
        }
    }

    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TImporter1, TImporter2, TImporter3, TImporter4, TImporter5, TImporter6, TImporter7, TImporter8, TImporter9, TImporter10, TImporter11, TImporter12, TImporter13> :
        IPyObjectImporter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>
        where TImporter1 : IPyObjectImporter<T1>
        where TImporter2 : IPyObjectImporter<T2>
        where TImporter3 : IPyObjectImporter<T3>
        where TImporter4 : IPyObjectImporter<T4>
        where TImporter5 : IPyObjectImporter<T5>
        where TImporter6 : IPyObjectImporter<T6>
        where TImporter7 : IPyObjectImporter<T7>
        where TImporter8 : IPyObjectImporter<T8>
        where TImporter9 : IPyObjectImporter<T9>
        where TImporter10 : IPyObjectImporter<T10>
        where TImporter11 : IPyObjectImporter<T11>
        where TImporter12 : IPyObjectImporter<T12>
        where TImporter13 : IPyObjectImporter<T13>
    {
        public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)
            Import(PyObject obj)
        {
            CheckTuple(obj);
            using var a = GetTupleItem(obj, 0);
            using var b = GetTupleItem(obj, 1);
            using var c = GetTupleItem(obj, 2);
            using var d = GetTupleItem(obj, 3);
            using var e = GetTupleItem(obj, 4);
            using var f = GetTupleItem(obj, 5);
            using var g = GetTupleItem(obj, 6);
            using var h = GetTupleItem(obj, 7);
            using var i = GetTupleItem(obj, 8);
            using var j = GetTupleItem(obj, 9);
            using var k = GetTupleItem(obj, 10);
            using var l = GetTupleItem(obj, 11);
            using var m = GetTupleItem(obj, 12);
            return (TImporter1.Import(a), TImporter2.Import(b), TImporter3.Import(c), TImporter4.Import(d), TImporter5.Import(e), TImporter6.Import(f), TImporter7.Import(g), TImporter8.Import(h), TImporter9.Import(i), TImporter10.Import(j), TImporter11.Import(k), TImporter12.Import(l), TImporter13.Import(m));
        }
    }
}
