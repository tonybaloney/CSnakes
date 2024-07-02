using Python.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace PythonEnvironments;

public static class PythonCollection
{
    public static IEnumerable<T> AsEnumerable<T>(this PyObject obj) =>
        new PyIterable(obj.GetIterator()).Select(item => item.As<T>());
}
