using CSnakes.Runtime.EnvironmentManagement;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.PackageManagement;

internal class UVInstaller(ILogger<UVInstaller> logger, string requirementsFileName) : IPythonPackageInstaller
{
    static readonly string binaryName = $"uv{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    public Task InstallPackages(string home, IEnvironmentManagement? environmentManager)
    {
        string requirementsPath = Path.GetFullPath(Path.Combine(home, requirementsFileName));
        if (File.Exists(requirementsPath))
        {
            logger.LogDebug("File {Requirements} was found.", requirementsPath);
            InstallPackagesWithUv(home, environmentManager, $"-r {requirementsFileName} --verbose", logger);
        }
        else
        {
            logger.LogWarning("File {Requirements} was not found.", requirementsPath);
        }

        return Task.CompletedTask;
    }

    static internal void InstallPackagesWithUv(string home, IEnvironmentManagement? environmentManager, string requirements, ILogger logger)
    {
        string fileName = binaryName;
        string workingDirectory = home;
        string path = "";
        string arguments = $"pip install {requirements}";

        if (environmentManager is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentManager.GetPath());
            logger.LogDebug("Using virtual environment at {VirtualEnvironmentLocation} to install packages with uv.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            string uvPath = Path.Combine(venvScriptPath, binaryName);

            // Is UV installed?
            if (!File.Exists(uvPath))
            {
                // Install it with pip
                PipInstaller.InstallPackagesWithPip(home, environmentManager, "uv", logger);
            }

            fileName = uvPath;
            path = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
            IPythonPackageInstaller.ExecuteProcess(fileName, arguments, workingDirectory, path, logger, virtualEnvironmentLocation);
        } else
        {
            IPythonPackageInstaller.ExecuteProcess(fileName, arguments, workingDirectory, path, logger);
        }

    }
}
