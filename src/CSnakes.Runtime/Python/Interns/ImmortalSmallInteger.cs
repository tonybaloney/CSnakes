using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal class ImmortalSmallInteger(int value) : ImmortalPyObject(GetSmallIntHandle(value))
{
    private static nint GetSmallIntHandle(int value)
    {
        using (GIL.Acquire())
            return CPythonAPI.PyLong_FromLong(value);
    }
}
