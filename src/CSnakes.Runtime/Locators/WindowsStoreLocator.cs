using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;
internal class WindowsStoreLocator(Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    public override PythonLocationMetadata LocatePython()
    {
        string programFolder = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => $"Python{Version.Major}{Version.Minor}-arm64",
            Architecture.X64 => $"Python{Version.Major}{Version.Minor}",
            Architecture.X86 => $"Python{Version.Major}{Version.Minor}",
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{RuntimeInformation.ProcessArchitecture}'.")
        };
        var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", programFolder);
        return LocatePythonInternal(windowsStorePath);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
