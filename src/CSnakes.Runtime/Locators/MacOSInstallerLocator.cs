using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;
internal class MacOSInstallerLocator(Version version, bool freeThreaded = false) : PythonLocator
{
    protected override Version Version { get; } = version;

    public override PythonLocationMetadata LocatePython()
    {
        string framework = freeThreaded ? "PythonT.framework" : "Python.framework";
        string mappedVersion = $"{Version.Major}.{Version.Minor}";
        string pythonPath = Path.Combine($"/Library/Frameworks/{framework}/Versions", mappedVersion);
        return LocatePythonInternal(pythonPath, freeThreaded);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}
