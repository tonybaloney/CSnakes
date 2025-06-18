using CSnakes.Runtime.EnvironmentManagement;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.PackageManagement;

internal class UVInstaller(ILogger<UVInstaller>? logger, IEnvironmentManagement? environmentManager, string requirementsFileName) : IPythonPackageInstaller
{
    static readonly string binaryName = $"uv{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    public Task InstallPackagesFromRequirements(string home) => InstallPackagesFromRequirements(home, requirementsFileName);

    public Task InstallPackagesFromRequirements(string home, string fileName)
    {
        string requirementsPath = Path.GetFullPath(Path.Combine(home, fileName));
        if (File.Exists(requirementsPath))
        {
            logger?.LogDebug("File {Requirements} was found.", fileName);
            RunUvPipInstall(home, environmentManager, ["-r", requirementsFileName], logger);
        }
        else
        {
            logger?.LogWarning("File {Requirements} was not found.", fileName);
        }

        return Task.CompletedTask;
    }

    public Task InstallPackage(string package) => InstallPackages([package]);

    public Task InstallPackages(string[] packages)
    {
        RunUvPipInstall(Directory.GetCurrentDirectory(), environmentManager, packages, logger);
        return Task.CompletedTask;
    }

    static private void RunUvPipInstall(string home, IEnvironmentManagement? environmentManager, string[] requirements, ILogger? logger)
    {
        string fileName = binaryName;
        string workingDirectory = home;
        string path = "";
        string[] arguments = ["pip", "install", .. requirements, "--color", "never"];

        if (environmentManager is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentManager.GetPath());
            logger?.LogDebug("Using virtual environment at {VirtualEnvironmentLocation} to install packages with uv.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            string uvPath = Path.Combine(venvScriptPath, binaryName);

            // Is UV installed?
            if (!File.Exists(uvPath))
            {
                // Install it with pip
                PipInstaller.InstallPackageWithPip(home, environmentManager, "uv", logger);
            }

            fileName = uvPath;
            path = string.Join(Path.PathSeparator, venvScriptPath, Environment.GetEnvironmentVariable("PATH"));
            IReadOnlyDictionary<string, string?> extraEnv = new Dictionary<string, string?>
            {
                { "VIRTUAL_ENV", virtualEnvironmentLocation },
                { "UV_CACHE_DIR", Environment.GetEnvironmentVariable("UV_CACHE_DIR") },
                { "UV_NO_CACHE", Environment.GetEnvironmentVariable("UV_NO_CACHE") }
            };

            ProcessUtils.ExecuteProcess(fileName, arguments, workingDirectory, path, logger, extraEnv);
        }
        else
        {
            ProcessUtils.ExecuteProcess(fileName, arguments, workingDirectory, path, logger);
        }

    }
}
