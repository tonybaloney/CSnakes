using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PyObject ImportModule(string module)
    {
        return CPythonAPI.Import(module);
    }
}
