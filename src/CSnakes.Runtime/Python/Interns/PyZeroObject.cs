using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyZeroObject() : ImmortalPyObject(CPythonAPI.PyLong_FromLong(0))
{
    public override bool IsNone() => false;
    public override string GetRepr() => ToString();
    public override string ToString() => "0";
    internal override PyObject Clone() => this;
}
