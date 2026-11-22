using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal static class PlatformDetector
{
    /// <summary>
    /// Gets the <see cref="OSPlatform"/> of the operating system the process is running on.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">The operating system is not Windows, macOS or Linux.</exception>
    internal static OSPlatform GetCurrentPlatform() =>
        OperatingSystem.IsWindows() ? OSPlatform.Windows :
        OperatingSystem.IsMacOS() ? OSPlatform.OSX :
        OperatingSystem.IsLinux() ? OSPlatform.Linux :
        throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
}
