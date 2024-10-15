namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    internal static bool IsPyGenerator(MPyOPtr p)
    {
        // TODO : Find a reference to a generator object.
        return HasAttr(p, "__next__") && HasAttr(p, "send");
    }
}
