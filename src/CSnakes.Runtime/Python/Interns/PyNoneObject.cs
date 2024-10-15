using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyNoneObject : ImmortalPyObject
{
    public PyNoneObject() : base(CAPI.GetNone())
    {
    }

    public override bool IsNone() => true;

    public override string GetRepr() => ToString();

    public override string ToString() => "None";

    internal override PythonObject Clone() => this;
}
