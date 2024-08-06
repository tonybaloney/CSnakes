using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Python.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

internal class PythonEnvironment : IPythonEnvironment
{
    public PythonEnvironment(IEnumerable<PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers, PythonEnvironmentOptions options)
    {
        var location = locators
            .Where(locator => locator.IsSupported())
            .Select(locator => locator.LocatePython())
            .FirstOrDefault(loc => loc is not null)
            ?? throw new InvalidOperationException("Python installation not found.");

        string home = options.Home;

        if (!Directory.Exists(home))
        {
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (options.EnsureVirtualEnvironment)
        {
            EnsureVirtualEnvironment(location, options.VirtualEnvironmentPath);
        }

        foreach (var installer in packageInstallers)
        {
            installer.InstallPackages(home, options.VirtualEnvironmentPath);
        }

        string versionPath = MapVersion(location.Version);
        string majorVersion = MapVersion(location.Version, ".");
        // Don't use BinaryFormatter by default as it's deprecated in .NET 8
        // See https://github.com/pythonnet/pythonnet/issues/2282
        RuntimeData.FormatterFactory = () =>
        {
            return new NoopFormatter();
        };

        char sep = Path.PathSeparator;

        SetupStandardLibrary(location.Folder, versionPath, majorVersion, sep);

        if (!string.IsNullOrEmpty(home))
        {
            PythonEngine.PythonPath = PythonEngine.PythonPath + sep + home;
        }

        if (options.ExtraPaths is { Length: > 0 })
        {
            PythonEngine.PythonPath = PythonEngine.PythonPath + sep + string.Join(sep, options.ExtraPaths);
        }

        try
        {
            PythonEngine.Initialize();
        }
        catch (NullReferenceException)
        {

        }
    }

    private static void EnsureVirtualEnvironment(PythonLocationMetadata pythonLocation, string? venvPath)
    {
        if (venvPath is null)
        {
            throw new ArgumentNullException(nameof(venvPath), "Virtual environment location is not set.");
        }

        if (!Directory.Exists(venvPath))
        {
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = pythonLocation.Folder,
                FileName = "python",
                Arguments = $"-m venv {venvPath}"
            };
            using Process process = new() { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }
    }

    private static void SetupStandardLibrary(string pythonLocation, string versionPath, string majorVersion, char sep)
    {
        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, $"python{versionPath}.dll");
            PythonEngine.PythonPath = Path.Combine(pythonLocation, "Lib") + sep + Path.Combine(pythonLocation, "DLLs");
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.dylib");
            PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
            return;
        }

        Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.so");
        PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
    }

    internal static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

}
