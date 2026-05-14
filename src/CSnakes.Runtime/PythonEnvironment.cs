#if NET9_0_OR_GREATER
// https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock#guidelines
global using Lock = System.Threading.Lock;
#else
global using Lock = object;
#endif

using CSnakes.Runtime.CPython;
using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

internal class PythonEnvironment : IPythonEnvironment
{
    public ILogger<IPythonEnvironment>? Logger { get; private set; }

    private readonly CPythonAPI api;
    private bool disposedValue;
    private IAsyncDisposable? pythonCaptureLogger;

    private static IPythonEnvironment? pythonEnvironment;
    private readonly static Lock locker = new();

    public static IPythonEnvironment GetPythonEnvironment(IEnumerable<PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers, PythonEnvironmentOptions options, ILogger<IPythonEnvironment>? logger, IEnvironmentManagement? environmentManager = null)
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
        ILogger<IPythonEnvironment>? logger,
        IEnvironmentManagement? environmentManager = null)
    {
        Logger = logger;

        var location = locators
            .Where(locator => locator.IsSupported())
            .Select(locator => locator.LocatePython())
            .FirstOrDefault(loc => loc is not null);

        if (location is null)
        {
            logger?.LogError("Python installation not found. There were {LocatorCount} locators registered.", locators.Count());
            throw new InvalidOperationException("Python installation not found.");
        }

        string home = options.Home;
        string[] extraPaths = options.ExtraPaths;

        home = Path.GetFullPath(home);
        if (!Directory.Exists(home))
        {
            logger?.LogError("Python home directory does not exist: {Home}", home);
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (environmentManager is not null)
        {

            extraPaths = [.. options.ExtraPaths, environmentManager.GetExtraPackagePath(location!)];

            environmentManager.EnsureEnvironment(location);
        }

        logger?.LogDebug("Setting up Python environment from {PythonLocation} using home of {Home}", location.Folder, home);

        foreach (var installer in packageInstallers)
        {
            installer.InstallPackagesFromRequirements(home);
        }

        char sep = Path.PathSeparator;

        api = SetupCPythonAPI(location, options);

        if (!string.IsNullOrEmpty(home))
        {
            api.PythonPath = api.PythonPath + sep + home;
        }

        if (extraPaths is { Length: > 0 })
        {
            logger?.LogDebug("Adding extra paths to PYTHONPATH: {ExtraPaths}", extraPaths);
            api.PythonPath = api.PythonPath + sep + string.Join(sep, extraPaths);
        }
        api.Initialize();

        if (options.CaptureLogs)
        {
            if (logger is null)
                throw new ArgumentNullException(nameof(logger), "Argument cannot be null when capturing Python logs.");
            pythonCaptureLogger = PythonLogger.EnableGlobalLogging(this, logger);
        }
    }

    private CPythonAPI SetupCPythonAPI(PythonLocationMetadata pythonLocationMetadata, PythonEnvironmentOptions options)
    {
        string pythonDll = pythonLocationMetadata.LibPythonPath;
        string pythonPath = pythonLocationMetadata.PythonPath;

        Logger?.LogDebug("Python DLL: {PythonDLL}", pythonDll);
        Logger?.LogDebug("Python path: {PythonPath}", pythonPath);

        var api = new CPythonAPI(pythonDll, pythonLocationMetadata.Version, pythonLocationMetadata.PythonBinaryPath, options.InstallSignalHandlers)
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
                pythonCaptureLogger?.DisposeAsync().GetAwaiter().GetResult();
                this.disposal?.Fire();
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

    Event<IPythonEnvironment>? disposal;

    public IDisposable AddDisposalListener<TArg>(TArg arg, Action<IPythonEnvironment, IDisposable, TArg> handler) =>
        (disposal ??= new(this)).Subscribe(arg, handler);

    sealed class Event<T>(T sender)
    {
        readonly List<Subscription?> subscriptions = new();
        bool firing;

        public IDisposable Subscribe<TArg>(TArg arg, Action<T, IDisposable, TArg> handler)
        {
            var subscription = new Subscription<TArg>(this, arg, handler);
            subscriptions.Add(subscription);
            return subscription;
        }

        void Remove(Subscription subscription)
        {
            if (firing)
            {
                if (subscriptions.IndexOf(subscription) is var index and >= 0)
                    subscriptions[index] = null;
            }
            else
            {
                subscriptions.Remove(subscription);
            }
        }

        public void Fire()
        {
            firing = true;

            try
            {
                for (int i = 0; i < subscriptions.Count; i++)
                    subscriptions[i]?.Fire(sender);
            }
            finally
            {
                firing = false;
                subscriptions.RemoveAll(s => s is null);
            }
        }

        abstract class Subscription(Event<T> @event) : IDisposable
        {
            public abstract void Fire(T sender);
            public void Dispose() => @event.Remove(this);
        }

        sealed class Subscription<TArg>(Event<T> @event, TArg arg, Action<T, IDisposable, TArg> handler) :
            Subscription(@event)
        {
            public override void Fire(T sender) => handler(sender, this, arg);
        }
    }
}
