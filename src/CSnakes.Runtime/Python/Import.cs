using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PyObject ImportModule(string module)
    {
        using (GIL.Acquire())
            return CPythonAPI.Import(module);
    }

    public static void ReloadModule(ref PyObject module)
    {
        using (GIL.Acquire())
        {
            var newModule = CPythonAPI.ReloadModule(module);
            module.Dispose();
            module = newModule;
        }
    }
}
