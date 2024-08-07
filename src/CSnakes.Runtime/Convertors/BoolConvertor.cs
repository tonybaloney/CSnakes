using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;
public class BoolConvertor : IPythonConvertor<bool>
{
    public bool CanEncode(Type type) =>
        type == typeof(bool);

    public bool CanDecode(PyObject objectType, Type targetType) =>
        targetType == typeof(bool) && CPythonAPI.IsPyBool(objectType.DangerousGetHandle());

    public bool TryDecode(PyObject pyObj, out bool value)
    {
        if (!CPythonAPI.IsPyBool(pyObj.DangerousGetHandle()))
        {
            value = false;
            return false;
        }
        value = true;
        return true;
    }

    public bool TryEncode(bool value, out PyObject? result)
    {
        result = new PyObject(CPythonAPI.PyBool_FromLong(Convert.ToInt64(value)));
        return true;
    }
}
