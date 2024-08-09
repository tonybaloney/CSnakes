using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal class SourceLocator(string folder, string version, bool debug = true) : PythonLocator(version: version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var buildFolder = Path.Combine(folder, "PCbuild", "amd64");
        return new PythonLocationMetadata(
            buildFolder,
            version,
            debug
        );
    }

    // TODO: Implement other platforms.
    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
