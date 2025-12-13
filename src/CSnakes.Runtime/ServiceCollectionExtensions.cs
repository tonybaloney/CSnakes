using CSnakes.Runtime.EnvironmentManagement;
using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CSnakes.Runtime;
/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Python-related services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Python-related services to the service collection with the specified Python home directory.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="home">The Python home directory.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IPythonEnvironmentBuilder WithPython(this IServiceCollection services)
    {
        var pythonBuilder = new PythonEnvironmentBuilder(services);

        services.AddSingleton<IPythonEnvironmentBuilder>(pythonBuilder);
        services.AddSingleton<IPythonEnvironment>(sp =>
        {
            var envBuilder = sp.GetRequiredService<IPythonEnvironmentBuilder>();
            var locators = sp.GetServices<PythonLocator>();
            var installers = sp.GetServices<IPythonPackageInstaller>();
            var logger = sp.GetService<ILogger<IPythonEnvironment>>();
            var environmentManager = sp.GetService<IEnvironmentManagement>();

            var options = envBuilder.GetOptions();

            return PythonEnvironment.GetPythonEnvironment(locators, installers, options, logger, environmentManager);
        });

        return pythonBuilder;
    }

    public static Version ParsePythonVersion(string version)
    {
        // Remove non -numeric characters except .
        Match versionMatch = VersionParseExpr().Match(version);
        if (!versionMatch.Success)
        {
            throw new InvalidOperationException($"Invalid Python version: '{version}'");
        }

        if (!Version.TryParse(versionMatch.Value, out Version? parsed))
        {
            throw new InvalidOperationException($"Failed to parse Python version: '{version}'");
        }

        if (parsed.Build == -1)
        {
            return new Version(parsed.Major, parsed.Minor, 0, 0);
        }

        if (parsed.Revision == -1)
        {
            return new Version(parsed.Major, parsed.Minor, parsed.Build, 0);
        }

        return parsed;
    }

    /// <summary>
    /// Adds a Python locator using Python from a NuGet packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the NuGet package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromNuGet(this IPythonEnvironmentBuilder builder, string version)
    {
        // See https://github.com/tonybaloney/CSnakes/issues/154#issuecomment-2352116849
        version = version.Replace("alpha.", "a").Replace("beta.", "b").Replace("rc.", "rc");

        // If a supplied version only consists of 2 tokens - e.g., 1.10 or 2.14 - then append an extra token
        if (version.Count(c => c == '.') < 2)
        {
            version = $"{version}.0";
        }

        builder.Services.AddSingleton<PythonLocator>(new NuGetLocator(version, ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Windows Store packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Store package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromWindowsStore(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new WindowsStoreLocator(ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Windows Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromWindowsInstaller(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new WindowsInstallerLocator(ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Official macOS Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromMacOSInstallerLocator(this IPythonEnvironmentBuilder builder, string version, bool freeThreaded = false)
    {
        builder.Services.AddSingleton<PythonLocator>(new MacOSInstallerLocator(ParsePythonVersion(version), freeThreaded));
        return builder;
    }

    public static IPythonEnvironmentBuilder FromSource(this IPythonEnvironmentBuilder builder, string folder, string version, bool debug = true, bool freeThreaded = false)
    {
        builder.Services.AddSingleton<PythonLocator>(new SourceLocator(folder, ParsePythonVersion(version), debug, freeThreaded));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from an environment variable to the service collection with the specified environment variable name and version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="environmentVariable">The name of the environment variable.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromEnvironmentVariable(this IPythonEnvironmentBuilder builder, string environmentVariable, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new EnvironmentVariableLocator(environmentVariable, ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from a specific folder to the service collection with the specified folder path and version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="folder">The path to the folder.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromFolder(this IPythonEnvironmentBuilder builder, string folder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new FolderLocator(folder, ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from a conda environment
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="condaBinaryPath">The path to the conda binary.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromConda(this IPythonEnvironmentBuilder builder, string condaBinaryPath)
    {
        builder.Services.AddSingleton<CondaLocator>(
            sp =>
            {
                var logger = sp.GetService<ILogger<IPythonEnvironment>>();
                return new CondaLocator(logger, condaBinaryPath);
            }
        );
        builder.Services.AddSingleton<PythonLocator>(
            sp =>
            {
                var condaLocator = sp.GetRequiredService<CondaLocator>();
                return condaLocator;
            }
        );
        return builder;
    }

    /// <summary>
    /// Simplest option for getting started with CSnakes.
    /// Downloads and installs the redistributable version of Python 3.12 from GitHub and stores it in %APPDATA%/csnakes.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="debug">Whether to use the debug version of Python.</param>
    /// <param name="timeout">Timeout in seconds for the download and installation process.</param>
    /// <returns></returns>
#pragma warning disable RS0026 // TODO Do not add multiple public overloads with optional parameters
    public static IPythonEnvironmentBuilder FromRedistributable(this IPythonEnvironmentBuilder builder, bool debug = false, int timeout = 360)
#pragma warning restore RS0026
    {
        builder.Services.AddSingleton<PythonLocator>(
            sp =>
            {
                var logger = sp.GetService<ILogger<RedistributableLocator>>();
                return new RedistributableLocator(logger, RedistributablePythonVersion.Python3_12, timeout, debug);
            }
        );
        return builder;
    }

    /// <summary>
    /// Simplest option for getting started with CSnakes.
    /// Downloads and installs the redistributable version of Python from GitHub and stores it in %APPDATA%/csnakes.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the redistributable Python to use.</param>
    /// <param name="debug">Whether to use the debug version of Python.</param>
    /// <param name="freeThreaded">Free Threaded Python (3.13+ only)</param>
    /// <param name="timeout">Timeout in seconds for the download and installation process.</param>
    /// <returns></returns>
#pragma warning disable RS0026 // TODO Do not add multiple public overloads with optional parameters
    public static IPythonEnvironmentBuilder FromRedistributable(this IPythonEnvironmentBuilder builder, RedistributablePythonVersion version, bool debug = false, bool freeThreaded = false, int timeout = 360)
#pragma warning restore RS0026
    {
        builder.Services.AddSingleton<PythonLocator>(
            sp =>
            {
                var logger = sp.GetService<ILogger<RedistributableLocator>>();
                return new RedistributableLocator(logger, version, timeout, debug, freeThreaded);
            }
        );
        return builder;
    }

    /// <summary>
    /// Simplest option for getting started with CSnakes.
    /// Downloads and installs the redistributable version of Python from GitHub and stores it in %APP_DATA%/csnakes.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the redistributable Python to use, e.g. "3.13"</param>
    /// <param name="debug">Whether to use the debug version of Python.</param>
    /// <param name="freeThreaded">Free Threaded Python (3.13+ only)</param>
    /// <param name="timeout">Timeout in seconds for the download and installation process.</param>
    /// <returns></returns>
#pragma warning disable RS0026 // TODO Do not add multiple public overloads with optional parameters
    public static IPythonEnvironmentBuilder FromRedistributable(this IPythonEnvironmentBuilder builder, string version, bool debug = false, bool freeThreaded = false, int timeout = 360)
#pragma warning restore RS0026
    {
        RedistributablePythonVersion versionEnum = version switch
        {
            "3.9" => RedistributablePythonVersion.Python3_9,
            "3.10" => RedistributablePythonVersion.Python3_10,
            "3.11" => RedistributablePythonVersion.Python3_11,
            "3.12" => RedistributablePythonVersion.Python3_12,
            "3.13" => RedistributablePythonVersion.Python3_13,
            "3.14" => RedistributablePythonVersion.Python3_14,
            _ => throw new ArgumentException($"Invalid major Python version: {version}. Try something like '3.12' or '3.13'. Only versions 3.9-3.14 are supported."),
        };
        return FromRedistributable(builder, versionEnum, debug, freeThreaded, timeout);
    }


    /// <summary>
    /// Adds a pip package installer to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the installer to.</param>
    /// <param name="requirementsPath">The path to the requirements file.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder WithPipInstaller(this IPythonEnvironmentBuilder builder, string requirementsPath = "requirements.txt")
    {
        builder.Services.AddSingleton<IPythonPackageInstaller>(
            sp =>
            {
                var logger = sp.GetService<ILogger<PipInstaller>>();
                var environmentManager = sp.GetService<IEnvironmentManagement>();
                return new PipInstaller(logger, environmentManager, requirementsPath);
            }
        );
        return builder;
    }

    /// <summary>
    /// Adds a uv package installer to the service collection. If uv is not installed, it will be installed with pip.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the installer to.</param>
    /// <param name="requirementsPath">The path to the requirements file.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder WithUvInstaller(this IPythonEnvironmentBuilder builder, string requirementsPath = "requirements.txt")
    {
        builder.Services.AddSingleton<IPythonPackageInstaller>(
            sp =>
            {
                var logger = sp.GetService<ILogger<UVInstaller>>();
                var environmentManager = sp.GetService<IEnvironmentManagement>();
                return new UVInstaller(logger, environmentManager, requirementsPath);
            }
        );
        return builder;
    }

    [GeneratedRegex("^(\\d+(\\.\\d+)*)")]
    private static partial Regex VersionParseExpr();
}
