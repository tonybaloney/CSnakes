using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
    private static IntPtr PyUnicodeType = IntPtr.Zero;

    internal static nint AsPyUnicodeObject(string s)
    {
        fixed (char* c = s)
        {
            return PyUnicode_DecodeUTF16(c, s.Length * sizeof(char), 0, 0);
        }
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial nint PyUnicode_DecodeUTF16(char* str, nint size, IntPtr errors, IntPtr byteorder);

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    internal static partial string? PyUnicode_AsUTF8(IntPtr s);

    public static bool IsPyUnicode(nint p)
    {
        return PyObject_IsInstance(p, PyUnicodeType);
    }
}
