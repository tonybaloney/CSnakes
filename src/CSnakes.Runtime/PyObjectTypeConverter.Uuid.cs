using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    internal static PyObject ConvertFromGuid(Guid guid)
    {
        using PyObject uuidModule = CPythonAPI.Import("uuid");
        using PyObject uuidClass = uuidModule.GetAttr("UUID");
        using PyObject guidStr = PyObject.Create(CPythonAPI.AsPyUnicodeObject(guid.ToString()));
        return uuidClass.Call(guidStr);
    }

    internal static Guid ConvertToGuid(PyObject pyObject) => Guid.Parse(pyObject.ToString());
}
