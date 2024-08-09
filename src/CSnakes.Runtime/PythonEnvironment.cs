using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

internal class PythonEnvironment : IPythonEnvironment
{
    private readonly CPythonAPI api;
    private bool disposedValue;

    private static IPythonEnvironment? pythonEnvironment;
    private readonly static object locker = new();

    public static IPythonEnvironment GetPythonEnvironment(IEnumerable<PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers, PythonEnvironmentOptions options)
    {
            if (pythonEnvironment is null)
            {
                lock(locker)
                {
                    pythonEnvironment ??= new PythonEnvironment(locators, packageInstallers, options);
                }
            }
            return pythonEnvironment;
    }

    private PythonEnvironment(
        IEnumerable<PythonLocator> locators,
        IEnumerable<IPythonPackageInstaller> packageInstallers,
        PythonEnvironmentOptions options)
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

        char sep = Path.PathSeparator;

        api = SetupStandardLibrary(location.Folder, versionPath, majorVersion, sep);

        if (!string.IsNullOrEmpty(home))
        {
            api.PythonPath = api.PythonPath + sep + home;
        }

        if (options.ExtraPaths is { Length: > 0 })
        {
            api.PythonPath = api.PythonPath + sep + string.Join(sep, options.ExtraPaths);
        }
        api.Initialize();
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

    private static CPythonAPI SetupStandardLibrary(string pythonLocation, string versionPath, string majorVersion, char sep)
    {
        string pythonDll = string.Empty;
        string pythonPath = string.Empty;

        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pythonDll = Path.Combine(pythonLocation, $"python{versionPath}.dll");
            pythonPath = Path.Combine(pythonLocation, "Lib") + sep + Path.Combine(pythonLocation, "DLLs");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            pythonDll = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.dylib");
            pythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pythonDll = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.so");
            pythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
        var api = new CPythonAPI(pythonDll)
        {
            PythonPath = pythonPath
        };
        return api;
    }

    internal static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                api.Dispose();
                if (pythonEnvironment is not null)
                {
                    lock(locker)
                    {
                        if (pythonEnvironment is not null)
                        {
                            pythonEnvironment = null;
                        }
                    }
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
