using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal class SourceLocator(string folder, string version, bool debug = true, bool freeThreaded = false) : PythonLocator(version: version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var buildFolder = Path.Combine(folder, "PCbuild", "amd64");
        return new PythonLocationMetadata(
            buildFolder,
            version,
            debug,
            freeThreaded
        );
    }

    // TODO: Implement other platforms.
    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
