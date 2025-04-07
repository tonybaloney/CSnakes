namespace CSnakes.Runtime.Python;
partial class InternalServices
{
    partial class Converters
    {
        public sealed class Tuple<T1, T2, TConverter1, TConverter2> :
            IConverter<(T1, T2)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
        {
            public static (T1, T2)
                Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var a = GetTupleItem(obj, 0);
                using var b = GetTupleItem(obj, 1);
                return (TConverter1.Convert(a), TConverter2.Convert(b));
            }
        }

        public sealed class Tuple<T1, T2, T3, TConverter1, TConverter2, TConverter3> :
            IConverter<(T1, T2, T3)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
        {
            public static (T1, T2, T3)
                Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var a = GetTupleItem(obj, 0);
                using var b = GetTupleItem(obj, 1);
                using var c = GetTupleItem(obj, 2);
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, TConverter1, TConverter2, TConverter3, TConverter4> :
            IConverter<(T1, T2, T3, T4)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
        {
            public static (T1, T2, T3, T4)
                Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var a = GetTupleItem(obj, 0);
                using var b = GetTupleItem(obj, 1);
                using var c = GetTupleItem(obj, 2);
                using var d = GetTupleItem(obj, 3);
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5> :
            IConverter<(T1, T2, T3, T4, T5)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
        {
            public static (T1, T2, T3, T4, T5)
                Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var a = GetTupleItem(obj, 0);
                using var b = GetTupleItem(obj, 1);
                using var c = GetTupleItem(obj, 2);
                using var d = GetTupleItem(obj, 3);
                using var e = GetTupleItem(obj, 4);
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6> :
            IConverter<(T1, T2, T3, T4, T5, T6)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
        {
            public static (T1, T2, T3, T4, T5, T6)
                Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var a = GetTupleItem(obj, 0);
                using var b = GetTupleItem(obj, 1);
                using var c = GetTupleItem(obj, 2);
                using var d = GetTupleItem(obj, 3);
                using var e = GetTupleItem(obj, 4);
                using var f = GetTupleItem(obj, 5);
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
        {
            public static (T1, T2, T3, T4, T5, T6, T7)
                Convert(PyObject obj)
            {
                CheckTuple(obj);
                using var a = GetTupleItem(obj, 0);
                using var b = GetTupleItem(obj, 1);
                using var c = GetTupleItem(obj, 2);
                using var d = GetTupleItem(obj, 3);
                using var e = GetTupleItem(obj, 4);
                using var f = GetTupleItem(obj, 5);
                using var g = GetTupleItem(obj, 6);
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7, TConverter8> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7, T8)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
            where TConverter8 : IConverter<T8>
        {
            public static (T1, T2, T3, T4, T5, T6, T7, T8)
                Convert(PyObject obj)
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
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g), TConverter8.Convert(h));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7, TConverter8, TConverter9> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
            where TConverter8 : IConverter<T8>
            where TConverter9 : IConverter<T9>
        {
            public static (T1, T2, T3, T4, T5, T6, T7, T8, T9)
                Convert(PyObject obj)
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
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g), TConverter8.Convert(h), TConverter9.Convert(i));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7, TConverter8, TConverter9, TConverter10> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
            where TConverter8 : IConverter<T8>
            where TConverter9 : IConverter<T9>
            where TConverter10 : IConverter<T10>
        {
            public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)
                Convert(PyObject obj)
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
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g), TConverter8.Convert(h), TConverter9.Convert(i), TConverter10.Convert(j));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7, TConverter8, TConverter9, TConverter10, TConverter11> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
            where TConverter8 : IConverter<T8>
            where TConverter9 : IConverter<T9>
            where TConverter10 : IConverter<T10>
            where TConverter11 : IConverter<T11>
        {
            public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)
                Convert(PyObject obj)
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
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g), TConverter8.Convert(h), TConverter9.Convert(i), TConverter10.Convert(j), TConverter11.Convert(k));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7, TConverter8, TConverter9, TConverter10, TConverter11, TConverter12> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
            where TConverter8 : IConverter<T8>
            where TConverter9 : IConverter<T9>
            where TConverter10 : IConverter<T10>
            where TConverter11 : IConverter<T11>
            where TConverter12 : IConverter<T12>
        {
            public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)
                Convert(PyObject obj)
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
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g), TConverter8.Convert(h), TConverter9.Convert(i), TConverter10.Convert(j), TConverter11.Convert(k), TConverter12.Convert(l));
            }
        }

        public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverter7, TConverter8, TConverter9, TConverter10, TConverter11, TConverter12, TConverter13> :
            IConverter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>
            where TConverter1 : IConverter<T1>
            where TConverter2 : IConverter<T2>
            where TConverter3 : IConverter<T3>
            where TConverter4 : IConverter<T4>
            where TConverter5 : IConverter<T5>
            where TConverter6 : IConverter<T6>
            where TConverter7 : IConverter<T7>
            where TConverter8 : IConverter<T8>
            where TConverter9 : IConverter<T9>
            where TConverter10 : IConverter<T10>
            where TConverter11 : IConverter<T11>
            where TConverter12 : IConverter<T12>
            where TConverter13 : IConverter<T13>
        {
            public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)
                Convert(PyObject obj)
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
                return (TConverter1.Convert(a), TConverter2.Convert(b), TConverter3.Convert(c), TConverter4.Convert(d), TConverter5.Convert(e), TConverter6.Convert(f), TConverter7.Convert(g), TConverter8.Convert(h), TConverter9.Convert(i), TConverter10.Convert(j), TConverter11.Convert(k), TConverter12.Convert(l), TConverter13.Convert(m));
            }
        }
    }
}
