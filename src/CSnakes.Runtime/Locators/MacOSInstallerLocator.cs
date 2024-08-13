using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;
internal class MacOSInstallerLocator(Version version) : PythonLocator(version)
{
    public override PythonLocationMetadata LocatePython()
    {
        string mappedVersion = $"{Version.Major}.{Version.Minor}";
        string pythonPath = Path.Combine("/Library/Frameworks/Python.framework/Versions", mappedVersion);
        return LocatePythonInternal(pythonPath);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}
