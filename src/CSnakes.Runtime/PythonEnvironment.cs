using CSnakes.Runtime.CPython;
using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

internal class PythonEnvironment : IPythonEnvironment
{
    public ILogger<IPythonEnvironment> Logger { get; private set; }

    private readonly CPythonAPI api;
    private bool disposedValue;

    private static IPythonEnvironment? pythonEnvironment;
    private readonly static object locker = new();

    public static IPythonEnvironment GetPythonEnvironment(IEnumerable<PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers, PythonEnvironmentOptions options, ILogger<IPythonEnvironment> logger, IEnvironmentManagement? environmentManager = null)
    {
        if (pythonEnvironment is null)
        {
            lock (locker)
            {
                pythonEnvironment ??= new PythonEnvironment(locators, packageInstallers, options, logger, environmentManager);
            }
        }
        return pythonEnvironment;
    }

    private PythonEnvironment(
        IEnumerable<PythonLocator> locators,
        IEnumerable<IPythonPackageInstaller> packageInstallers,
        PythonEnvironmentOptions options,
        ILogger<IPythonEnvironment> logger,
        IEnvironmentManagement? environmentManager = null)
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
        string[] extraPaths = options.ExtraPaths;

        home = Path.GetFullPath(home);
        if (!Directory.Exists(home))
        {
            logger.LogError("Python home directory does not exist: {Home}", home);
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (environmentManager is not null) {
            
            extraPaths = [.. options.ExtraPaths, environmentManager.GetExtraPackagePath(logger, location!)];

            environmentManager.EnsureEnvironment(logger, location);
        }

        logger.LogInformation("Setting up Python environment from {PythonLocation} using home of {Home}", location.Folder, home);

        foreach (var installer in packageInstallers)
        {
            installer.InstallPackages(home, environmentManager);
        }

        char sep = Path.PathSeparator;

        api = SetupStandardLibrary(location);

        if (!string.IsNullOrEmpty(home))
        {
            api.PythonPath = api.PythonPath + sep + home;
        }

        if (extraPaths is { Length: > 0 })
        {
            logger.LogDebug("Adding extra paths to PYTHONPATH: {ExtraPaths}", extraPaths);
            api.PythonPath = api.PythonPath + sep + string.Join(sep, extraPaths);
        }
        api.Initialize();
    }

    private CPythonAPI SetupStandardLibrary(PythonLocationMetadata pythonLocationMetadata)
    {
        string pythonDll = pythonLocationMetadata.LibPythonPath;
        string pythonPath = pythonLocationMetadata.PythonPath;

        Logger.LogDebug("Python DLL: {PythonDLL}", pythonDll);
        Logger.LogDebug("Python path: {PythonPath}", pythonPath);

        var api = new CPythonAPI(pythonDll, pythonLocationMetadata.Version)
        {
            PythonPath = pythonPath
        };
        return api;
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
