namespace CSnakes.Runtime.CPython;
internal unsafe partial class CAPI
{
    internal static byte[] ByteArrayFromPyBytes(MPyOPtr ob) => ByteArrayFromPyBytes(ob.DangerousGetHandle());
}
