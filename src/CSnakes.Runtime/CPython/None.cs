namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    internal static bool IsNone(MPyOPtr o)
    {
        return _PyNone == o.DangerousGetHandle();
    }
}
