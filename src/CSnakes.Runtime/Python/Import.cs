using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PyObject ImportModule(string module)
    {
        return CPythonAPI.Import(module);
    }

    public static PyObject ReloadModule(PyObject module)
    {
        var newModule = CPythonAPI.ReloadModule(module);
        module.Dispose();
        return newModule;
    }
}
