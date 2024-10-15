namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Calls PyUnicode_AsUTF8 and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static string StringFromPyUnicodeToUTF8(MPyOPtr s) => StringFromPyUnicodeToUTF8(s.DangerousGetHandle());
}
