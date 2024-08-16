using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal class SourceLocator(string folder, Version version, bool debug = true, bool freeThreaded = false) : PythonLocator(version)
{
    protected bool Debug => debug;

    public override PythonLocationMetadata LocatePython()
    {
        var buildFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(folder, "PCbuild", "amd64") : folder;

        if (string.IsNullOrEmpty(buildFolder) || !Directory.Exists(buildFolder))
        {
            throw new DirectoryNotFoundException($"Python {Version} not found in {buildFolder}.");
        }

        return LocatePythonInternal(folder, freeThreaded);
    }
}
