using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;

public class DoubleConvertor : IPythonConvertor
{
    public bool CanEncode(Type type) =>
        typeof(double).IsAssignableFrom(type);

    public bool CanDecode(PyObject objectType, Type type) =>
        typeof(double).IsAssignableFrom(type) && CPythonAPI.IsPyFloat(objectType.DangerousGetHandle());

    public bool TryEncode(object value, out PyObject? result)
    {
        result = new(CPythonAPI.PyFloat_FromDouble((double)value));
        return true;
    }

    public bool TryDecode(PyObject value, out object? result)
    {
        if (!CPythonAPI.IsPyFloat(value.DangerousGetHandle()))
        {
            result = -0.0;
            return false;
        }

        result = CPythonAPI.PyFloat_AsDouble(value.DangerousGetHandle());
        return true;
    }
}