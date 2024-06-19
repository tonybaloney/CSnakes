using Python.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace PythonEnvironments
{
    public static class PythonLists
    {
        public static List<T> AsList<T>(this PyObject obj)
        {
            return new PyIterable(obj.GetIterator()).Select(item => item.As<T>()).ToList();
        }
    }
}
