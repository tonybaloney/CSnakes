using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyNegativeOneObject() : ImmortalPyObject(CPythonAPI.PyLong_FromLong(-1))
{
    public override bool IsNone() => false;
    public override string GetRepr() => ToString();
    public override string ToString() => "-1";
    internal override PyObject Clone() => this;
}
