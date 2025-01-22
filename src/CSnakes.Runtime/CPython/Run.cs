using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;

internal unsafe partial class CPythonAPI
{
    internal enum InputType
    {
        Py_single_input = 256,
        Py_file_input = 257,
        Py_eval_input = 258
    }

    internal static PyObject PyRun_String(string str, InputType start, PyObject globals, PyObject locals) => PyObject.Create(PyRun_String_(str, start, globals, locals));

    [LibraryImport(PythonLibraryName, EntryPoint = "PyRun_String", StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    private static partial nint PyRun_String_(string str, InputType start, PyObject globals, PyObject locals);
}
