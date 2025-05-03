using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;

namespace CSnakes.Runtime;

public sealed class PythonEnvironmentConfiguration : IServiceProvider
{
    public static PythonEnvironmentConfiguration Default { get; } =
        new([], [],
            new PythonEnvironmentOptions(AppContext.BaseDirectory, []),
            NullLoggerFactory.Instance,
            null);

    private PythonEnvironmentConfiguration(PythonEnvironmentConfiguration other) :
        this(other.Locators, other.PackageInstallers, other.Options, other.LoggerFactory, other.EnvironmentManager) { }

    private PythonEnvironmentConfiguration(ImmutableArray<PythonLocator> locators,
                                           ImmutableArray<IPythonPackageInstaller> packageInstallers,
                                           PythonEnvironmentOptions options,
                                           ILoggerFactory loggerFactory,
                                           IEnvironmentManagement? environmentManager)
    {
        Locators = locators;
        PackageInstallers = packageInstallers;
        Options = options;
        LoggerFactory = loggerFactory;
        EnvironmentManager = environmentManager;
    }

    public ImmutableArray<PythonLocator> Locators { get; init; }
    public ImmutableArray<IPythonPackageInstaller> PackageInstallers { get; init; }
    public PythonEnvironmentOptions Options { get; init; }
    public ILoggerFactory LoggerFactory { get; init; }
    public IEnvironmentManagement? EnvironmentManager { get; init; }

    public PythonEnvironmentConfiguration WithLocators(ImmutableArray<PythonLocator> value) =>
        new(this) { Locators = value };

    public PythonEnvironmentConfiguration AddLocator(PythonLocator locator) =>
        WithLocators(Locators.Add(locator));

    public PythonEnvironmentConfiguration WithPackageInstallers(ImmutableArray<IPythonPackageInstaller> value) =>
        new(this) { PackageInstallers = value };

    public PythonEnvironmentConfiguration AddPackageInstaller(IPythonPackageInstaller value) =>
        WithPackageInstallers(PackageInstallers.Add(value));

    public PythonEnvironmentConfiguration WithOptions(PythonEnvironmentOptions value) =>
        new(this) { Options = value };

    public PythonEnvironmentConfiguration WithLogger(ILoggerFactory value) =>
        new(this) { LoggerFactory = value };

    public PythonEnvironmentConfiguration WithEnvironmentManager(IEnvironmentManagement? value) =>
        new(this) { EnvironmentManager = value };

    private static Version ParsePythonVersion(string version) =>
        ServiceCollectionExtensions.ParsePythonVersion(version);

    /// <summary>
    /// Adds a Python locator using Python from a NuGet packages to the service collection with the specified version.
    /// </summary>
    /// <param name="version">The version of the NuGet package.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromNuGet(string version) =>
        ServiceCollectionExtensions.FromNuGet(version, this, static (self, locator) => self.AddLocator(locator));

    /// <summary>
    /// Adds a Python locator using Python from the Windows Store packages to the service collection with the specified version.
    /// </summary>
    /// <param name="version">The version of the Windows Store package.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromWindowsStore(string version) =>
        AddLocator(new WindowsStoreLocator(ParsePythonVersion(version)));

    /// <summary>
    /// Adds a Python locator using Python from the Windows Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromWindowsInstaller(string version) =>
        AddLocator(new WindowsInstallerLocator(ParsePythonVersion(version)));

    /// <summary>
    /// Adds a Python locator using Python from the Official macOS Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <param name="freeThreaded">Whether to use the free-threaded build</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromMacOSInstallerLocator(string version, bool freeThreaded = false) =>
        AddLocator(new MacOSInstallerLocator(ParsePythonVersion(version), freeThreaded));

    public PythonEnvironmentConfiguration FromSource(string folder, string version, bool debug = true, bool freeThreaded = false) =>
        AddLocator(new SourceLocator(folder, ParsePythonVersion(version), debug, freeThreaded));

    /// <summary>
    /// Adds a Python locator using Python from an environment variable to the service collection with the specified environment variable name and version.
    /// </summary>
    /// <param name="environmentVariable">The name of the environment variable.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromEnvironmentVariable(string environmentVariable, string version) =>
        AddLocator(new EnvironmentVariableLocator(environmentVariable, ParsePythonVersion(version)));

    /// <summary>
    /// Adds a Python locator using Python from a specific folder to the service collection with the specified folder path and version.
    /// </summary>
    /// <param name="folder">The path to the folder.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromFolder(string folder, string version) =>
        AddLocator(new FolderLocator(folder, ParsePythonVersion(version)));

    /// <summary>
    /// Adds a Python locator using Python from a Conda environment
    /// </summary>
    /// <param name="condaBinaryPath">The path to the conda binary.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromConda(string condaBinaryPath) =>
        AddLocator(new CondaLocator(LoggerFactory.CreateLogger<CondaLocator>(), condaBinaryPath));

    /// <summary>
    /// Adds a pip package installer to the service collection.
    /// </summary>
    /// <param name="requirementsPath">The path to the requirements file.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration AddPipInstaller(string requirementsPath = "requirements.txt") =>
        AddPackageInstaller(new PipInstaller(LoggerFactory.CreateLogger<PipInstaller>(), requirementsPath));

    /// <summary>
    /// Simplest option for getting started with CSnakes. Downloads and installs the redistributable
    /// version of Python 3.12 from GitHub and stores it in <c>%APP_DATA%\csnakes</c>.
    /// </summary>
    /// <param name="debug">Whether to use the debug version of Python.</param>
    /// <param name="timeout">Timeout in seconds for the download and installation process.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromRedistributable(bool debug = false, int timeout = 360) =>
        FromRedistributable(RedistributablePythonVersion.Python3_12, debug, timeout: timeout);

    /// <summary>
    /// Simplest option for getting started with CSnakes.
    /// Downloads and installs the redistributable version of Python from GitHub and stores it in
    /// <c>%APP_DATA%/csnakes</c>.
    /// </summary>
    /// <param name="version">The version of the redistributable Python to use, e.g. "3.13"</param>
    /// <param name="debug">Whether to use the debug version of Python.</param>
    /// <param name="freeThreaded">Free Threaded Python (3.13+ only)</param>
    /// <param name="timeout">Timeout in seconds for the download and installation process.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromRedistributable(string version, bool debug = false, bool freeThreaded = false, int timeout = 360) =>
        FromRedistributable(debug: debug, freeThreaded: freeThreaded, timeout: timeout, version: version switch
        {
            "3.9" => RedistributablePythonVersion.Python3_9,
            "3.10" => RedistributablePythonVersion.Python3_10,
            "3.11" => RedistributablePythonVersion.Python3_11,
            "3.12" => RedistributablePythonVersion.Python3_12,
            "3.13" => RedistributablePythonVersion.Python3_13,
            "3.14" => RedistributablePythonVersion.Python3_14,
            _ => throw new ArgumentException($"Invalid major Python version: {version}. Try something like '3.12' or '3.13'. Only versions 3.9-3.14 are supported."),
        });

    /// <summary>
    /// Simplest option for getting started with CSnakes.
    /// Downloads and installs the redistributable version of Python from GitHub and stores it in
    /// <c>%APP_DATA%/csnakes</c>.
    /// </summary>
    /// <param name="version">The version of the redistributable Python to use.</param>
    /// <param name="debug">Whether to use the debug version of Python.</param>
    /// <param name="freeThreaded">Free Threaded Python (3.13+ only)</param>
    /// <param name="timeout">Timeout in seconds for the download and installation process.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration FromRedistributable(RedistributablePythonVersion version, bool debug = false, bool freeThreaded = false, int timeout = 360) =>
        AddLocator(new RedistributableLocator(LoggerFactory.CreateLogger<RedistributableLocator>(), version, timeout, debug, freeThreaded));

    /// <summary>
    /// Adds a uv package installer to the service collection. If uv is not installed, it will be installed with pip.
    /// </summary>
    /// <param name="requirementsPath">The path to the requirements file.</param>
    /// <returns>The modified <see cref="PythonEnvironmentConfiguration"/>.</returns>
    public PythonEnvironmentConfiguration AddUvInstaller(string requirementsPath = "requirements.txt") =>
        AddPackageInstaller(new UVInstaller(LoggerFactory.CreateLogger<UVInstaller>(), requirementsPath));

    public PythonEnvironmentConfiguration SetVirtualEnvironment(string path, bool ensureExists = true) =>
        WithEnvironmentManager(new VenvEnvironmentManagement(LoggerFactory.CreateLogger<VenvEnvironmentManagement>(), path, ensureExists));

    public PythonEnvironmentConfiguration SetCondaEnvironment(string name, string? environmentSpecPath = null, bool ensureEnvironment = false, string? pythonVersion = null) =>
        WithEnvironmentManager(new CondaEnvironmentManagement(LoggerFactory.CreateLogger<CondaEnvironmentManagement>(), name, ensureEnvironment, this.GetRequiredService<CondaLocator>(), environmentSpecPath, pythonVersion));

    public PythonEnvironmentConfiguration SetHome(string home) =>
        new(Locators, PackageInstallers, Options with { Home = home }, LoggerFactory, EnvironmentManager);

    public IPythonEnvironment GetPythonEnvironment() =>
        Locators.IsEmpty
        ? FromRedistributable().GetPythonEnvironment()
        : PythonEnvironment.GetPythonEnvironment(Locators, PackageInstallers, Options,
                                                 LoggerFactory.CreateLogger<IPythonEnvironment>(),
                                                 EnvironmentManager);

    object ? IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(IEnumerable<PythonLocator>))
            return Locators;

        if (serviceType == typeof(CondaLocator))
            return Locators.OfType<CondaLocator>().FirstOrDefault();

        if (serviceType == typeof(IEnumerable<IPythonPackageInstaller>))
            return PackageInstallers;

        if (serviceType == typeof(PythonEnvironmentOptions))
            return Options;

        if (serviceType == typeof(ILoggerFactory))
            return LoggerFactory;

        if (serviceType == typeof(IEnvironmentManagement))
            return EnvironmentManager;

        if (serviceType == typeof(IPythonEnvironment))
            return GetPythonEnvironment();

        return null;
    }
}
