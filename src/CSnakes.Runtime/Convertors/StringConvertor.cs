using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;

public class StringConvertor : IPythonConvertor
{
    public bool CanEncode(Type type) =>
        type == typeof(string);
    public bool CanDecode(PyObject objectType, Type targetType) =>
        targetType == typeof(string) && CPythonAPI.IsPyUnicode(objectType.DangerousGetHandle());

    public bool TryEncode(object value, out PyObject? result)
    {
        result = new PyObject(CPythonAPI.AsPyUnicodeObject((string)value));
        return true;
    }

    public bool TryDecode(PyObject pyObj, out object? result)
    {
        if (!CPythonAPI.IsPyUnicode(pyObj.DangerousGetHandle()))
        {
            result = null;
            return false;
        }
        result = CPythonAPI.PyUnicode_AsUTF8(pyObj.DangerousGetHandle());
        return true;
    }
}