using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal class PyNoneObject : ImmortalPyObject
{
    public PyNoneObject() : base(CPythonAPI.GetNone())
    {
    }

    public override bool IsNone()
    {
        return true;
    }

    public override string GetRepr()
    {
        return "None";
    }

    public override string ToString()
    {
        return "None";
    }
}
