using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Converters;

public class StringConvertor : IPythonConvertor<string>
{
    public bool CanEncode(Type type) =>
        type == typeof(string);
    public bool CanDecode(PyObject objectType, Type targetType) =>
        targetType == typeof(string) && CPythonAPI.IsPyUnicode(objectType.DangerousGetHandle());

    public bool TryEncode(string value, out PyObject? result)
    {
        result = new PyObject(CPythonAPI.AsPyUnicodeObject(value));
        return true;
    }

    public bool TryDecode(PyObject pyObj, out string? result)
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