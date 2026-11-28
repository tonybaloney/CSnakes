using CSnakes.Runtime.Python;
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

    /// <summary>
    /// Compare two strings: -1, 0 or 1 for less than, equal and greater than. Only for objects of type str; for others,
    /// -1 and an exception set.
    /// </summary>
    [LibraryImport(PythonLibraryName)]
    internal static partial int PyUnicode_Compare(nint left, nint right);

    /// <summary>
    /// Calls PyUnicode_AsUTF8 and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static string PyUnicode_AsUTF8(PyObject s)
    {
        var result = PyUnicode_AsUTF8_(s);
        return result is null ? throw PyObject.ThrowPythonExceptionAsClrException() : result;
    }

    /// <summary>
    /// Convert the string object to a UTF-8 encoded string and return a pointer to the internal buffer.
    /// This function does a type check and returns null if the object is not a string.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller), EntryPoint = "PyUnicode_AsUTF8")]
    private static partial string? PyUnicode_AsUTF8_(PyObject s);

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller), EntryPoint = "PyUnicode_AsUTF8")]
    internal static partial string? PyUnicode_AsUTF8Raw(nint s);

    public static bool IsPyUnicode(PyObject p)
    {
        return PyObject_IsInstance(p, PyUnicodeType);
    }
}
