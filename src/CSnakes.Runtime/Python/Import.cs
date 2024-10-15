using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PythonObject ImportModule(string module)
    {
        return CAPI.Import(module);
    }
}
