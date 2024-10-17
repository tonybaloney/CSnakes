using CSnakes.Runtime.Python.Interns;

namespace CSnakes.Runtime.Python;
public partial class PyObject
{
    public static PyObject None { get; } = new PyNoneObject();
    public static PyObject True { get; } = new PyTrueObject();
    public static PyObject False { get; } = new PyFalseObject();
    public static PyObject One { get; } = new PyOneObject();
    public static PyObject Zero { get; } = new PyZeroObject();
    public static PyObject NegativeOne { get; } = new PyNegativeOneObject();

}
