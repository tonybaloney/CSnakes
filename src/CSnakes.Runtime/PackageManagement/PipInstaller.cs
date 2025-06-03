using CSnakes.Runtime.EnvironmentManagement;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.PackageManagement;

internal class PipInstaller(ILogger<PipInstaller>? logger, IEnvironmentManagement? environmentManager, string requirementsFileName) : IPythonPackageInstaller
{
    static readonly string pipBinaryName = $"pip{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    public Task InstallPackages(string home)
    {
        string requirementsPath = Path.GetFullPath(Path.Combine(home, requirementsFileName));
        if (File.Exists(requirementsPath))
        {
            logger?.LogDebug("File {Requirements} was found.", requirementsPath);
            RunPipInstall(home, environmentManager, ["-r", requirementsFileName], logger);
        }
        else
        {
            logger?.LogWarning("File {Requirements} was not found.", requirementsPath);
        }

        return Task.CompletedTask;
    }

    public Task InstallPackage(string home, string package)
    {
        RunPipInstall(home, environmentManager, [package], logger);

        return Task.CompletedTask;
    }

    internal static void InstallPackageWithPip(string home, IEnvironmentManagement? environmentManager, string requirement, ILogger? logger)
        => RunPipInstall(home, environmentManager, [requirement], logger);

    private static void RunPipInstall(string home, IEnvironmentManagement? environmentManager, string[] requirements, ILogger? logger)
    {
        string fileName = pipBinaryName;
        string workingDirectory = home;
        string path = "";
        string[] arguments = [ "install", .. requirements, "--disable-pip-version-check" ];

        if (environmentManager is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentManager.GetPath());
            logger?.LogDebug("Using virtual environment at {VirtualEnvironmentLocation} to install packages with pip.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            // TODO: Check that the pip executable exists, and if not, raise an exception with actionable steps.
            fileName = Path.Combine(venvScriptPath, pipBinaryName);
            path = string.Join(Path.PathSeparator, venvScriptPath, Environment.GetEnvironmentVariable("PATH"));
            IReadOnlyDictionary<string, string?> extraEnv = new Dictionary<string, string?>
            {
                { "VIRTUAL_ENV", virtualEnvironmentLocation }
            };
            ProcessUtils.ExecuteProcess(fileName, arguments, workingDirectory, path, logger, extraEnv);
        }
        else
        {
            ProcessUtils.ExecuteProcess(fileName, arguments, workingDirectory, path, logger);
        }
    }
}
