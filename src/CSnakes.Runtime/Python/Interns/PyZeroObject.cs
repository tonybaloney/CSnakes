namespace CSnakes.Runtime.Python.Interns;

internal sealed class PyZeroObject() : ImmortalSmallInteger(0)
{
    public override bool IsNone() => false;
    public override string GetRepr() => ToString();
    public override string ToString() => "0";
    internal override PyObject Clone() => this;
}
