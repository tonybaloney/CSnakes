namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Calls PyLong_AsLongLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long LongLongFromPyLong(MPyOPtr p) => LongLongFromPyLong(p.DangerousGetHandle());

    /// <summary>
    /// Calls PyLong_AsLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long LongFromPyLong(MPyOPtr p) => LongFromPyLong(p.DangerousGetHandle());
}
