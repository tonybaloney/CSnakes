using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Unicode
{
    public static string? AsString(PyObject ob)
    {
        return CPythonAPI.PyUnicode_AsUTF8(ob.DangerousGetHandle());
    }

    public static PyObject FromString(string str)
    {
        return new PyObject(CPythonAPI.AsPyUnicodeObject(str));
    }
}
