using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PySmallIntObject : ImmortalPyObject
{
    private readonly short value;

    public PySmallIntObject(short value) : base(CPythonAPI.PyLong_FromLong(value))
    {
        this.value = value;
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => value.ToString();

    internal override PyObject Clone() => this;
}
