using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    internal static PyObject ConvertFromDateOnly(DateOnly date)
    {
        using PyObject datetimeModule = CPythonAPI.Import("datetime");
        using PyObject dateClass = datetimeModule.GetAttr("date");
        using PyObject fromOrdinalMethod = dateClass.GetAttr("fromordinal");
        using PyObject ordinalArg = PyObject.Create(CPythonAPI.PyLong_FromLongLong(date.DayNumber + 1L));
        return fromOrdinalMethod.Call(ordinalArg);
    }

    internal static DateOnly ConvertToDateOnly(PyObject pyObject)
    {
        using PyObject toOrdinalMethod = pyObject.GetAttr("toordinal");
        using PyObject ordinalObj = toOrdinalMethod.Call();
        long ordinal = CPythonAPI.PyLong_AsLongLong(ordinalObj);
        return DateOnly.FromDayNumber((int)(ordinal - 1));
    }
}
