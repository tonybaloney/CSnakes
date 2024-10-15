namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    public static bool IsBytes(MPyOPtr ob) => IsInstance(ob, _PyBytesType);

    internal static byte[] ByteArrayFromPyBytes(MPyOPtr ob) => ByteArrayFromPyBytes(ob.DangerousGetHandle());
}
