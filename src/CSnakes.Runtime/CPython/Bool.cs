namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    public static bool IsPyBool(MPyOPtr p)
    {
        return p.DangerousGetHandle() == _PyTrue || p.DangerousGetHandle() == _PyFalse;
    }

    public static bool IsPyTrue(MPyOPtr p)
    {
        return p.DangerousGetHandle() == _PyTrue;
    }
}
