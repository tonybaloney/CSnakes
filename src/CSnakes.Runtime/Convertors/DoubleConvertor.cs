using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;

public class DoubleConvertor : IPythonConvertor<double>
{
    public bool CanEncode(Type type) =>
        typeof(double).IsAssignableFrom(type);

    public bool CanDecode(PyObject objectType, Type type) =>
        typeof(double).IsAssignableFrom(objectType) && CPythonAPI.IsPyFloat(objectType.DangerousGetHandle());

    public bool TryEncode(double value, out PyObject? result)
    {
        result = new(CPythonAPI.PyFloat_AsDouble(value));
        return true;
    }

    public bool TryDecode(PyObject value, out double? result)
    {
        if (!CPythonAPI.IsPyFloat(objectType.DangerousGetHandle()))
        {
            result = null;
            return false;
        }

        result = CPythonAPI.PyFloat_AsDouble(value.DangerousGetHandle());
        return true;
    }
}