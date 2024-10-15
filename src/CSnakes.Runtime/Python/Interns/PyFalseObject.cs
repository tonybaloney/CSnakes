using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyFalseObject() : ImmortalPyObject(GetFalseHandle())
{
    private static nint GetFalseHandle()
    {
        using (GIL.Acquire())
            return CPythonAPI.PyBool_FromLong(0);
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => "False";

    internal override PyObject Clone() => this;
}
