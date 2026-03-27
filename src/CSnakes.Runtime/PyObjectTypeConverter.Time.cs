using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    internal static PyObject ConvertFromTimeOnly(TimeOnly time)
    {
        using PyObject datetimeModule = CPythonAPI.Import("datetime");
        using PyObject timeClass = datetimeModule.GetAttr("time");
        using PyObject hourArg = PyObject.Create(CPythonAPI.PyLong_FromLongLong(time.Hour));
        using PyObject minuteArg = PyObject.Create(CPythonAPI.PyLong_FromLongLong(time.Minute));
        using PyObject secondArg = PyObject.Create(CPythonAPI.PyLong_FromLongLong(time.Second));
        // Python microsecond is 0–999999; C# splits ms and us
        using PyObject microsecondArg = PyObject.Create(
            CPythonAPI.PyLong_FromLongLong(time.Millisecond * 1000L + time.Microsecond));
        return timeClass.Call(hourArg, minuteArg, secondArg, microsecondArg);
    }

    internal static TimeOnly ConvertToTimeOnly(PyObject pyObject)
    {
        using PyObject hourObj = pyObject.GetAttr("hour");
        using PyObject minuteObj = pyObject.GetAttr("minute");
        using PyObject secondObj = pyObject.GetAttr("second");
        using PyObject microsecondObj = pyObject.GetAttr("microsecond");

        long us = CPythonAPI.PyLong_AsLongLong(microsecondObj);

        return new TimeOnly(
            (int)CPythonAPI.PyLong_AsLongLong(hourObj),
            (int)CPythonAPI.PyLong_AsLongLong(minuteObj),
            (int)CPythonAPI.PyLong_AsLongLong(secondObj),
            millisecond: (int)(us / 1000),
            microsecond: (int)(us % 1000));
    }
}
