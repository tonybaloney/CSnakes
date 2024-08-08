using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.CPython;

[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(NonFreeUtf8StringMarshaller))]
internal static unsafe class NonFreeUtf8StringMarshaller
{
    public static byte* ConvertToUnmanaged(string? managed) =>
        Utf8StringMarshaller.ConvertToUnmanaged(managed);

    public static string? ConvertToManaged(byte* unmanaged) =>
        Utf8StringMarshaller.ConvertToManaged(unmanaged);

    public static void Free(byte* unmanaged)
    {
        // Do nothing, string is static (const char*)
    }
}