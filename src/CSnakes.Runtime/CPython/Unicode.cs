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

    /// <summary>
    /// Calls PyUnicode_AsUTF8 and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static string PyUnicode_AsUTF8(PythonObject s)
    {
        var result = PyUnicode_AsUTF8_(s);
        return result is null ? throw PythonObject.ThrowPythonExceptionAsClrException() : result;
    }

    /// <summary>
    /// Convert the string object to a UTF-8 encoded string and return a pointer to the internal buffer.
    /// This function does a type check and returns null if the object is not a string.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller), EntryPoint = "PyUnicode_AsUTF8")]
    private static partial string? PyUnicode_AsUTF8_(PythonObject s);

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller), EntryPoint = "PyUnicode_AsUTF8")]
    internal static partial string? PyUnicode_AsUTF8Raw(nint s);

    public static bool IsPyUnicode(PythonObject p)
    {
        return PyObject_IsInstance(p, PyUnicodeType);
    }
}
