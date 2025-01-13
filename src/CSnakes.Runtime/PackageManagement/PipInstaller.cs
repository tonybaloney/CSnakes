using CSnakes.Runtime.EnvironmentManagement;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.PackageManagement;

internal class PipInstaller(ILogger<PipInstaller> logger, string requirementsFileName) : IPythonPackageInstaller
{
    static readonly string pipBinaryName = $"pip{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    public Task InstallPackages(string home, IEnvironmentManagement? environmentManager)
    {
        string requirementsPath = Path.GetFullPath(Path.Combine(home, requirementsFileName));
        if (File.Exists(requirementsPath))
        {
            logger.LogDebug("File {Requirements} was found.", requirementsPath);
            InstallPackagesWithPip(home, environmentManager, $"-r { requirementsFileName}", logger);
        }
        else
        {
            logger.LogWarning("File {Requirements} was not found.", requirementsPath);
        }

        return Task.CompletedTask;
    }

    internal static void InstallPackagesWithPip(string home, IEnvironmentManagement? environmentManager, string requirements, ILogger logger)
    {
        string fileName = pipBinaryName;
        string workingDirectory = home;
        string path = "";
        string arguments = $"install {requirements} --disable-pip-version-check";

        if (environmentManager is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentManager.GetPath());
            logger.LogDebug("Using virtual environment at {VirtualEnvironmentLocation} to install packages with pip.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            // TODO: Check that the pip executable exists, and if not, raise an exception with actionable steps.
            fileName = Path.Combine(venvScriptPath, pipBinaryName);
            path = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
        }
        IPythonPackageInstaller.ExecuteProcess(fileName, "-V", workingDirectory, path, logger);
        IPythonPackageInstaller.ExecuteProcess(fileName, arguments, workingDirectory, path, logger);
    }
}
