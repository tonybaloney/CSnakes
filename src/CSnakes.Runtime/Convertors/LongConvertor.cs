using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Convertors;

public class LongConvertor : IPythonConvertor<long>
{
    public bool CanEncode(Type type) =>
        typeof(long).IsAssignableFrom(type);

    public bool CanDecode(PyObject pyObject, Type type) =>
        CPythonAPI.IsPyLong(pyObject.DangerousGetHandle()) && typeof(long).IsAssignableFrom(type);

    public bool TryEncode(long value, out PyObject? pyObject)
    {
        pyObject = new(CPythonAPI.PyLong_FromLong(value));
        return true;
    }

    public bool TryDecode(PyObject pyObject, out long? value)
    {
        if (!CPythonAPI.IsPyLong(pyObject.DangerousGetHandle()))
        {
            value = null;
            return false;
        }

        value = CPythonAPI.PyLong_AsLong(pyObject.DangerousGetHandle());
        return true;
    }
}