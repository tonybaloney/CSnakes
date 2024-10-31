using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    public void AppendMissingPathsToSysPath(string[] paths)
    {
        var pyoPtrSysName = AsPyUnicodeObject("sys");
        var pyoPtrSysModule = PyImport_Import(pyoPtrSysName);
        Py_DecRefRaw(pyoPtrSysModule);
        if (pyoPtrSysModule == IntPtr.Zero) PyObject.ThrowPythonExceptionAsClrException();

        var pyoPtrPathAttr = AsPyUnicodeObject("path");
        var pyoPtrPathList = PyObject_GetAttrRaw(pyoPtrSysModule, pyoPtrPathAttr);
        Py_DecRefRaw(pyoPtrPathAttr);
        if (pyoPtrSysModule == IntPtr.Zero) PyObject.ThrowPythonExceptionAsClrException();

        foreach (var path in paths)
        {
            var pyoPtrPythonPath = AsPyUnicodeObject(path);

            bool found = false;
            for (int i = 0; i < PyList_SizeRaw(pyoPtrPathList); i++)
            {
                var pyoPtrSysPath = PyList_GetItemRaw(pyoPtrPathList, i);
                if (pyoPtrSysPath == IntPtr.Zero)
                    continue;
                found = PyObject_RichCompareBoolRaw(pyoPtrPythonPath, pyoPtrSysPath, RichComparisonType.Equal) == 0;
                if (found)
                    break;
            }

            if (found == false)
                PyList_Append(pyoPtrPathList, pyoPtrPythonPath);

            Py_DecRefRaw(pyoPtrPythonPath);
        }

        Py_DecRefRaw(pyoPtrPathList);
        Py_DecRefRaw(pyoPtrSysModule);
    }

    [LibraryImport(PythonLibraryName, EntryPoint = "PyObject_RichCompareBool")]
    internal static partial int PyObject_RichCompareBoolRaw(nint ob1, nint ob2, RichComparisonType comparisonType);

    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_Size")]
    internal static partial int PyList_SizeRaw(nint ob);

    [LibraryImport(PythonLibraryName, EntryPoint = "PyList_GetItem")]
    internal static partial int PyList_GetItemRaw(nint ob, int index);
}
