using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    internal static PyObject* AsPyUnicodeObject(string s)
    {
        fixed (char* c = s)
        {
            return PyUnicode_DecodeUTF16(c, s.Length * sizeof(char), 0, 0);
        }
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial PyObject* PyUnicode_DecodeUTF16(char* str, nint size, IntPtr errors, IntPtr byteorder);

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    internal static partial string? PyUnicode_AsUTF8(PyObject* s);
}
