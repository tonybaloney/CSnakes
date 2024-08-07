using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class CPythonAPI
{
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
        return PyUnicode_CheckExact(p) == 1 || PyUnicode_Check(p) == 1;
    }

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyUnicode_Check(nint p);

    [LibraryImport(PythonLibraryName)]
    internal static partial int PyUnicode_CheckExact(nint p);
}
