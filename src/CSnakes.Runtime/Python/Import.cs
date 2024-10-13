using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PyObject ImportModule(string module)
    {
        return CPythonAPI.Import(module);
    }

    public static void ReloadModule(ref PyObject module)
    {
        var newModule = CPythonAPI.ReloadModule(module);
        module.Dispose();
        module = newModule;
    }
}
