using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

internal class PythonEnvironment : IPythonEnvironment
{
    public ILogger<IPythonEnvironment> Logger { get; private set; }

    private readonly CPythonAPI api;
    private bool disposedValue;

    private static IPythonEnvironment? pythonEnvironment;
    private readonly static object locker = new();

    public static IPythonEnvironment GetPythonEnvironment(IEnumerable<PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers, PythonEnvironmentOptions options, Microsoft.Extensions.Logging.ILogger<IPythonEnvironment> logger)
    {
        if (pythonEnvironment is null)
        {
            lock (locker)
            {
                pythonEnvironment ??= new PythonEnvironment(locators, packageInstallers, options, logger);
            }
        }
        return pythonEnvironment;
    }

    private PythonEnvironment(
        IEnumerable<PythonLocator> locators,
        IEnumerable<IPythonPackageInstaller> packageInstallers,
        PythonEnvironmentOptions options,
        ILogger<IPythonEnvironment> logger)
    {
        Logger = logger;

        var location = locators
            .Where(locator => locator.IsSupported())
            .Select(locator => locator.LocatePython())
            .FirstOrDefault(loc => loc is not null);

        if (location is null)
        {
            logger.LogError("Python installation not found. There were {LocatorCount} locators registered.", locators.Count());
            throw new InvalidOperationException("Python installation not found.");
        }

        string home = options.Home;

        if (!Directory.Exists(home))
        {
            logger.LogError("Python home directory does not exist: {Home}", home);
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (options.EnsureVirtualEnvironment)
        {
            EnsureVirtualEnvironment(location, options.VirtualEnvironmentPath);
        }

        logger.LogInformation("Setting up Python environment from {PythonLocation} using home of {Home}", location.Folder, home);

        foreach (var installer in packageInstallers)
        {
            installer.InstallPackages(home, options.VirtualEnvironmentPath);
        }

        string versionPath = MapVersion(location.Version);
        string majorVersion = MapVersion(location.Version, ".");

        char sep = Path.PathSeparator;

        api = SetupStandardLibrary(location, versionPath, majorVersion, sep);

        if (!string.IsNullOrEmpty(home))
        {
            api.PythonPath = api.PythonPath + sep + home;
        }

        if (options.ExtraPaths is { Length: > 0 })
        {
            logger.LogInformation("Adding extra paths to PYTHONPATH: {ExtraPaths}", options.ExtraPaths);
            api.PythonPath = api.PythonPath + sep + string.Join(sep, options.ExtraPaths);
        }
        api.Initialize();
    }

    private void EnsureVirtualEnvironment(PythonLocationMetadata pythonLocation, string? venvPath)
    {
        if (venvPath is null)
        {
            Logger.LogError("Virtual environment location is not set but it was requested to be created.");
            throw new ArgumentNullException(nameof(venvPath), "Virtual environment location is not set.");
        }

        if (!Directory.Exists(venvPath))
        {
            Logger.LogInformation("Creating virtual environment at {VirtualEnvPath}", venvPath);

            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = pythonLocation.Folder,
                FileName = "python",
                Arguments = $"-m venv {venvPath}"
            };
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            using Process process = new() { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Logger.LogInformation("{Data}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Logger.LogError("{Data}", e.Data);
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }
    }

    private CPythonAPI SetupStandardLibrary(PythonLocationMetadata pythonLocationMetadata, string versionPath, string majorVersion, char sep)
    {
        string pythonDll = string.Empty;
        string pythonPath = string.Empty;
        string pythonLocation = pythonLocationMetadata.Folder;
        string suffix = String.Empty;

        if (pythonLocationMetadata.FreeThreaded)
        {
            suffix += "t";
        }

        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            suffix += pythonLocationMetadata.Debug ? "_d" : string.Empty;
            pythonDll = Path.Combine(pythonLocation, $"python{versionPath}{suffix}.dll");
            if (pythonLocationMetadata.Debug)
            {
                // From source..
                pythonPath = Path.Combine(pythonLocation, "..", "..", "Lib") + sep + pythonLocation;
            }
            else
            {
                pythonPath = Path.Combine(pythonLocation, "Lib") + sep + Path.Combine(pythonLocation, "DLLs");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            suffix += pythonLocationMetadata.Debug ? "d" : string.Empty;
            if (pythonLocationMetadata.Debug) // from source
            {
                pythonDll = Path.Combine(pythonLocation, $"libpython{majorVersion}{suffix}.dylib");
                pythonPath = Path.Combine(pythonLocation, "Lib"); // TODO : build/lib.macosx-13.6-x86_64-3.13-pydebug
            }
            else
            {
                pythonDll = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}{suffix}.dylib");
                pythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            pythonDll = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}{suffix}.so");
            pythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }

        Logger.LogInformation("Python DLL: {PythonDLL}", pythonDll);
        Logger.LogInformation("Python path: {PythonPath}", pythonPath);

        var api = new CPythonAPI(pythonDll, pythonLocationMetadata.FreeThreaded)
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
                    lock (locker)
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

    public bool IsDisposed()
    {
        return disposedValue;
    }
}
