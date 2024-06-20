using Python.Runtime;
using System;

namespace PythonEnvironments
{
    public static class PythonTuples
    {
        public static Tuple<T1, T2> AsTuple<T1, T2>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> AsTuple<T1, T2, T3>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            var item3 = obj[2].As<T3>();
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        public static Tuple<T1, T2, T3, T4> AsTuple<T1, T2, T3, T4>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            var item3 = obj[2].As<T3>();
            var item4 = obj[3].As<T4>();
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static Tuple<T1, T2, T3, T4, T5> AsTuple<T1, T2, T3, T4, T5>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            var item3 = obj[2].As<T3>();
            var item4 = obj[3].As<T4>();
            var item5 = obj[4].As<T5>();
            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6> AsTuple<T1, T2, T3, T4, T5, T6>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            var item3 = obj[2].As<T3>();
            var item4 = obj[3].As<T4>();
            var item5 = obj[4].As<T5>();
            var item6 = obj[5].As<T6>();
            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6, T7> AsTuple<T1, T2, T3, T4, T5, T6, T7>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            var item3 = obj[2].As<T3>();
            var item4 = obj[3].As<T4>();
            var item5 = obj[4].As<T5>();
            var item6 = obj[5].As<T6>();
            var item7 = obj[6].As<T7>();
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6, T7, T8> AsTuple<T1, T2, T3, T4, T5, T6, T7, T8>(this PyObject obj)
        {
            var item1 = obj[0].As<T1>();
            var item2 = obj[1].As<T2>();
            var item3 = obj[2].As<T3>();
            var item4 = obj[3].As<T4>();
            var item5 = obj[4].As<T5>();
            var item6 = obj[5].As<T6>();
            var item7 = obj[6].As<T7>();
            var item8 = obj[7].As<T8>();
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>(item1, item2, item3, item4, item5, item6, item7, item8);
        }

        // TODO : C# supports up to 21
    }
}
