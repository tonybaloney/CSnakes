using System.Runtime.InteropServices;

public static class PlatformHelpers {
    public static bool IsOSX() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static bool IsApple() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || OperatingSystem.IsOSPlatform("iOS");
}