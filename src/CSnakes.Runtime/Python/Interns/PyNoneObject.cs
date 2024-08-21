using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyNoneObject : ImmortalPyObject

{
    public PyNoneObject() : base(CPythonAPI.GetNone())
    {
    }

    public override bool IsNone() => true;

    public override string GetRepr() => ToString();

    public override string ToString() => "None";

}
