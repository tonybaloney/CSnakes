using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyTrueObject : ImmortalPyObject
{
    public PyTrueObject() : base(CPythonAPI.PyBool_FromLong(1))
    {
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => "True";

    internal override PythonObject Clone() => this;
}
