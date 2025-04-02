using System.Runtime.InteropServices;
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

[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(Utf32StringMarshaller))]
internal static class Utf32StringMarshaller
{
    public static IntPtr ConvertToUnmanaged(string? managed)
    {
        if (managed is null)
            return IntPtr.Zero;

        // Allocate memory for the UTF-32 string
        int byteCount = (managed.Length + 1) * 4;
        IntPtr unmanagedString = Marshal.AllocHGlobal(byteCount);

        // Copy the managed string to unmanaged memory
        for (int i = 0; i < managed.Length; i++)
        {
            Marshal.WriteInt32(unmanagedString, i * 4, managed[i]);
        }

        // Null-terminate the string
        Marshal.WriteInt32(unmanagedString, managed.Length * 4, 0);

        return unmanagedString;
    }

    public static string? ConvertToManaged(IntPtr unmanaged)
    {
        if (unmanaged == IntPtr.Zero)
            return null;

        // Determine the length of the string
        int length = 0;
        while (Marshal.ReadInt32(unmanaged, length * 4) != 0)
        {
            length++;
        }

        // Copy the unmanaged string to managed memory
        char[] managedString = new char[length];
        for (int i = 0; i < length; i++)
        {
            managedString[i] = (char)Marshal.ReadInt32(unmanaged, i * 4);
        }

        return new string(managedString);
    }

    public static void FreeUnmanaged(IntPtr unmanaged)
    {
        if (unmanaged != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(unmanaged);
        }
    }
}
