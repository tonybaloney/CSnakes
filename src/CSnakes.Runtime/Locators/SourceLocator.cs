using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

internal class SourceLocator(string folder, Version version, bool debug = true, bool freeThreaded = false) : PythonLocator(version)
{
    protected bool Debug => debug;

    protected override string GetLibPythonPath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{Version.Major}{Version.Minor}{suffix}{(debug ? "_d" : string.Empty)}.dll");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}{(debug ? "d" : string.Empty)}.dylib");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}{(debug ? "d" : string.Empty)}.so");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
    }

    protected override string GetPythonPath(string folder)
    {
        return Path.Combine(folder, "..", "..", "Lib") + Path.PathSeparator + folder;
    }

    protected override string GetPythonExecutablePath(string folder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Debug)
        {
            return Path.Combine(folder, "python_d.exe");
        }
        return base.GetPythonExecutablePath(folder);
    }

    public override PythonLocationMetadata LocatePython()
    {
        var buildFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(folder, "PCbuild", "amd64") : folder;

        if (string.IsNullOrEmpty(buildFolder) || !Directory.Exists(buildFolder))
        {
            throw new DirectoryNotFoundException($"Python {Version} not found in {buildFolder}.");
        }

        return LocatePythonInternal(buildFolder, freeThreaded);
    }

    internal override bool IsSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
