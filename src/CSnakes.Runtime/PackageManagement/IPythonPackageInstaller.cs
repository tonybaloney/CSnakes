namespace CSnakes.Runtime.PackageManagement;

/// <summary>
/// Represents an interface for installing Python packages.
/// </summary>
public interface IPythonPackageInstaller
{
    /// <summary>
    /// Install packages from a requirements file located in the specified home directory.
    /// </summary>
    /// <param name="home">The home directory where the requirements file is located.</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    Task InstallPackagesFromRequirements(string home);
    Task InstallPackagesFromRequirements(string home, string file);

    /// <summary>
    /// Install a single package.
    /// </summary>
    /// <param name="package">Name of the package to install.</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    Task InstallPackage(string package);

    /// <summary>
    /// Install multiple packages.
    /// </summary>
    /// <param name="packages">The packages to install.</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    Task InstallPackages(string[] packages);
}
