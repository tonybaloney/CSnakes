using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Locators;

/// <summary>
/// Abstract class for locating Python installations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PythonLocator"/> class.
/// </remarks>
/// <param name="version">The version of Python.</param>
public abstract class PythonLocator(Version version)
{
    /// <summary>
    /// Gets the version of Python.
    /// </summary>
    protected Version Version { get ; } = version;

    /// <summary>
    /// Locates the Python installation.
    /// </summary>
    /// <returns>The metadata of the located Python installation.</returns>
    public abstract PythonLocationMetadata LocatePython();

    protected string GetPythonExecutablePath(string folder)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, "python.exe");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "bin", "python3");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "bin", "python3");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
    }

    protected string GetLibPythonPath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{version.Major}{version.Minor}{suffix}.dll");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"libpython{version.Major}.{version.Minor}{suffix}.dylib");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"libpython{version.Major}.{version.Minor}{suffix}.so");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
    }

    /// <summary>
    /// Get the standard lib path for Python.
    /// </summary>
    /// <param name="folder">The base folder</param>
    /// <returns></returns>
    protected string GetPythonPath(string folder)
    {
        char sep = Path.PathSeparator;

        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, "Lib") + sep + Path.Combine(folder, "DLLs");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"python{version.Major}.{version.Minor}") + sep + Path.Combine(folder, "lib", $"python{version.Major}.{version.Minor}", "lib-dynload");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"python{version.Major}.{version.Minor}") + sep + Path.Combine(folder, "lib", $"python{version.Major}.{version.Minor}", "lib-dynload");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
    }

    /// <summary>
    /// Locates the Python installation internally.
    /// </summary>
    /// <param name="folder">The folder path to search for Python.</param>
    /// <returns>The metadata of the located Python installation.</returns>
    /// <exception cref="DirectoryNotFoundException">Python not found at the specified folder.</exception>
    protected PythonLocationMetadata LocatePythonInternal(string folder, bool freeThreaded = false)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Python not found at {folder}.");
        }

        return new PythonLocationMetadata(folder, Version, GetLibPythonPath(folder, freeThreaded), GetPythonPath(folder), GetPythonExecutablePath(folder));
    }

    /// <summary>
    /// Specifies whether the Python Locator is supported. Defaults to <see langword="true"/>
    /// </summary>
    /// <remarks>This would be used to do an OS check and ignore a particular <see cref="PythonLocator"/> if the OS does not match the one it supports. See the <see cref="WindowsInstallerLocator"/> as an example.</remarks>
    internal virtual bool IsSupported() => true;
}
