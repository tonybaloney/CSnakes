using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal class SourceLocator(string folder, string version, bool debug = true, bool freeThreaded = false) : PythonLocator(version: version)
{
    public override PythonLocationMetadata LocatePython()
    {
        var buildFolder = String.Empty;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
            buildFolder = Path.Combine(folder, "PCbuild", "amd64");
        } else {
            buildFolder = folder;
        }

        if (string.IsNullOrEmpty(buildFolder) || !Directory.Exists(buildFolder))
        {
            throw new DirectoryNotFoundException($"Python {Version} not found in {buildFolder}.");
        }

        return new PythonLocationMetadata(
            buildFolder,
            version,
            debug,
            freeThreaded
        );
    }
}
