using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyTrueObject() : ImmortalPyObject(GetTrueHandle())
{
    private static nint GetTrueHandle()
    {
        using (GIL.Acquire())
            return CPythonAPI.PyBool_FromLong(1);
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => "True";

    internal override PyObject Clone() => this;
}
