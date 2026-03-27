using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    internal static PyObject ConvertFromGuid(Guid guid)
    {
        using PyObject uuidModule = CPythonAPI.Import("uuid");
        using PyObject uuidClass = uuidModule.GetAttr("UUID");
        using PyObject bytesKeyword = PyObject.Create(CPythonAPI.AsPyUnicodeObject("bytes"));
        using PyObject bytesValue = PyObject.Create(CPythonAPI.PyBytes_FromByteSpan(guid.ToByteArray(true)));
        return uuidClass.Call([], [new KeywordArg(bytesKeyword, bytesValue)]);
    }

    internal static Guid ConvertToGuid(PyObject pyObject)
    {
        using PyObject bytesAttr = pyObject.GetAttr("bytes");
        return new Guid(CPythonAPI.PyBytes_AsByteArray(bytesAttr), true);
    }
}
