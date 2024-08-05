namespace CSnakes.Runtime.PackageManagement;

public interface IPythonPackageInstaller
{
    Task InstallPackages(string home, string? virtualEnvironmentLocation);
}