using CSnakes.Runtime.Locators;
using CSnakes.Runtime.PackageManagement;
using Microsoft.Extensions.DependencyInjection;

namespace CSnakes.Runtime;
/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Python-related services.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds Python-related services to the service collection with the specified Python home directory.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="home">The Python home directory.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPython(this IServiceCollection services, string home)
    {
        services.AddSingleton<IPythonEnvironmentBuilder, PythonEnvironmentBuilder>((sp) =>
        {
            var locators = sp.GetServices<PythonLocator>();
            var installers = sp.GetServices<IPythonPackageInstaller>();
            var builder = new PythonEnvironmentBuilder(locators, installers);
            builder.WithHome(home);
            return builder;
        });
        services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironmentBuilder>().Build());
        return services;
    }

    /// <summary>
    /// Adds Python-related services to the service collection with the specified Python home directory and virtual environment.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="home">The Python home directory.</param>
    /// <param name="venv">The virtual environment directory.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPython(this IServiceCollection services, string home, string venv)
    {
        services.AddSingleton<IPythonEnvironmentBuilder, PythonEnvironmentBuilder>((sp) =>
        {
            var locators = sp.GetServices<PythonLocator>();
            var installers = sp.GetServices<IPythonPackageInstaller>();
            var builder = new PythonEnvironmentBuilder(locators, installers);
            builder.WithHome(home);
            builder.WithVirtualEnvironment(venv);
            return builder;
        });
        services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironmentBuilder>().Build());
        return services;
    }

    /// <summary>
    /// Adds a Python locator using Python from a NuGet packages to the service collection with the specified version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the locator to.</param>
    /// <param name="version">The version of the NuGet package.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPythonFromNuGet(this IServiceCollection services, string version) =>
        services.AddSingleton<PythonLocator>(new NuGetLocator(version));

    /// <summary>
    /// Adds a Python locator using Python from the Windows Store packages to the service collection with the specified version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Store package.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPythonFromWindowsStore(this IServiceCollection services, string version) =>
        services.AddSingleton<PythonLocator>(new WindowsStoreLocator(version));

    /// <summary>
    /// Adds a Python locator using Python from the Windows Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPythonFromWindowsInstaller(this IServiceCollection services, string version) =>
        services.AddSingleton<PythonLocator>(new WindowsInstallerLocator(version));

    /// <summary>
    /// Adds a Python locator using Python from an environment variable to the service collection with the specified environment variable name and version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the locator to.</param>
    /// <param name="environmentVariable">The name of the environment variable.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPythonFromEnvironmentVariable(this IServiceCollection services, string environmentVariable, string version) =>
        services.AddSingleton<PythonLocator>(new EnvironmentVariableLocator(environmentVariable, version));

    /// <summary>
    /// Adds a Python locator using Python from a specific folder to the service collection with the specified folder path and version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the locator to.</param>
    /// <param name="folder">The path to the folder.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPythonFromFolder(this IServiceCollection services, string folder, string version) =>
        services.AddSingleton<PythonLocator>(new FolderLocator(folder, version));

    /// <summary>
    /// Adds a Python locator using Python from the system PATH to the service collection with the specified version.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the locator to.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPythonFromPath(this IServiceCollection services, string version) =>
        services.AddSingleton<PythonLocator>(new PathLocator(version));

    /// <summary>
    /// Adds a pip package installer to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the installer to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection WithPipInstaller(this IServiceCollection services) =>
        services.AddSingleton<IPythonPackageInstaller, PipInstaller>();
}
