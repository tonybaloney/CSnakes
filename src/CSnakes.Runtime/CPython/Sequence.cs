namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    public static bool IsPySequence(MPyOPtr p) => PySequence_Check(p) == 1;
}
