using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PythonObject ImportModule(string module)
    {
        return PythonObject.Create(CAPI.Import(module));
    }
}
