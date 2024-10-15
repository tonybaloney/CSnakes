using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyFalseObject : ImmortalPyObject
{
    public PyFalseObject() : base(CPythonAPI.PyBool_FromLong(0))
    {
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => "False";

    internal override PythonObject Clone() => this;
}
