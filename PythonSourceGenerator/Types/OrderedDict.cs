using Python.Runtime;

namespace PythonSourceGenerator.Types
{
    internal static class OrderedDict
    {
        public static PyIterable Items(this PyObject self)
        {
            var items = self.GetAttr("items").Invoke();
            return new PyIterable(items);
        }
    }
}
