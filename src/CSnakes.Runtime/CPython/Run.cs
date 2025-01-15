using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal static PyObject PyRun_String(string str, PyObject globals, PyObject locals) => PyObject.Create(PyRun_String_(str, 0, globals, locals));

    [LibraryImport(PythonLibraryName, EntryPoint = "PyRun_String", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    private static partial nint PyRun_String_(string str, int start, PyObject globals, PyObject locals);
}
