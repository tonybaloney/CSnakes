using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime;

internal class PythonEnvironment : IPythonEnvironment
{
    private readonly ILogger<IPythonEnvironment> logger;

    ILogger<IPythonEnvironment> IPythonEnvironment.Logger => logger;

    public PythonEnvironment(
        IEnumerable<PythonLocator> locators,
        IEnumerable<IPythonPackageInstaller> packageInstallers,
        PythonEnvironmentOptions options,
        ILogger<IPythonEnvironment> logger)
    {
        this.logger = logger;

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
            logger.LogInformation("Adding Python home directory to PYTHONPATH: {Home}", home);
            PythonEngine.PythonPath = PythonEngine.PythonPath + sep + home;
        }

        if (options.ExtraPaths is { Length: > 0 })
        {
            logger.LogInformation("Adding extra paths to PYTHONPATH: {ExtraPaths}", options.ExtraPaths);
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

    private void EnsureVirtualEnvironment(PythonLocationMetadata pythonLocation, string? venvPath)
    {
        if (venvPath is null)
        {
            logger.LogError("Virtual environment location is not set but it was requested to be created.");
            throw new ArgumentNullException(nameof(venvPath), "Virtual environment location is not set.");
        }

        if (!Directory.Exists(venvPath))
        {
            logger.LogInformation("Creating virtual environment at {VirtualEnvPath}", venvPath);
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = pythonLocation.Folder,
                FileName = "python",
                Arguments = $"-m venv {venvPath}"
            };
            using Process process = new() { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogInformation("{Data}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogError("{Data}", e.Data);
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }

    private void SetupStandardLibrary(string pythonLocation, string versionPath, string majorVersion, char sep)
    {
        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, $"python{versionPath}.dll");
            PythonEngine.PythonPath = Path.Combine(pythonLocation, "Lib") + sep + Path.Combine(pythonLocation, "DLLs");
            logger.LogInformation("Python DLL: {PythonDLL}", Python.Runtime.Runtime.PythonDLL);
            logger.LogInformation("Python path: {PythonPath}", PythonEngine.PythonPath);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.dylib");
            PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
            logger.LogInformation("Python DLL: {PythonDLL}", Python.Runtime.Runtime.PythonDLL);
            logger.LogInformation("Python path: {PythonPath}", PythonEngine.PythonPath);
            return;
        }

        Python.Runtime.Runtime.PythonDLL = Path.Combine(pythonLocation, "lib", $"libpython{majorVersion}.so");
        PythonEngine.PythonPath = Path.Combine(pythonLocation, "lib", $"python{majorVersion}") + sep + Path.Combine(pythonLocation, "lib", $"python{majorVersion}", "lib-dynload");
        logger.LogInformation("Python DLL: {PythonDLL}", Python.Runtime.Runtime.PythonDLL);
        logger.LogInformation("Python path: {PythonPath}", PythonEngine.PythonPath);
    }

    internal static string MapVersion(string version, string sep = "")
    {
        // split on . then take the first two segments and join them without spaces
        var versionParts = version.Split('.');
        return string.Join(sep, versionParts.Take(2));
    }

}
