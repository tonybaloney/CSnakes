using Python.Runtime;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection
{
    public static class ModuleReflection
    {
        public static string FromModule(PyObject moduleObject, PyModule scope)
        {
            var @classBody = "";
            // Get methods
            var moduleDir = moduleObject.Dir();
            var callables = new List<string>();
            foreach (PyObject attrName in moduleDir)
            {
                var attr = moduleObject.GetAttr(attrName);
                if (attr.IsCallable() && attr.HasAttr("__name__"))
                {
                    scope.Import("inspect");
                    string x = $"signature = inspect.signature({moduleObject.GetAttr("__name__")}.{attrName})";
                    scope.Exec(x);
                    var signature = scope.Get("signature");
                    var name = attr.GetAttr("__name__").ToString();
                    @classBody += MethodReflection.FromMethod(signature, name).ToFullString();
                    callables.Add(attr.ToString());
                }
            }
            return @classBody;
        }
    }
}
