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
            logger.LogInformation("File {Requirements} was found.", requirementsPath);
            InstallPackagesWithUv(home, environmentManager, $"-r {requirementsFileName}", logger);
        }
        else
        {
            logger.LogWarning("File {Requirements} was not found.", requirementsPath);
        }

        return Task.CompletedTask;
    }

    static internal void InstallPackagesWithUv(string home, IEnvironmentManagement? environmentManager, string requirements, ILogger logger)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = home,
            FileName = binaryName,
            Arguments = $"install {requirements}"
        };

        if (environmentManager is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentManager.GetPath());
            logger.LogInformation("Using virtual environment at {VirtualEnvironmentLocation} to install packages with uv.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            string uvPath = Path.Combine(venvScriptPath, binaryName);

            // Is UV installed?
            if (!File.Exists(uvPath))
            {
                // Install it with pip
                PipInstaller.InstallPackagesWithPip(home, environmentManager, "uv", logger);
            }

            startInfo.FileName = uvPath;
            startInfo.EnvironmentVariables["PATH"] = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
        }

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using Process process = new() { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                logger.LogInformation("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                logger.LogWarning("{Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            logger.LogError("Failed to install packages.");
            throw new InvalidOperationException("Failed to install packages.");
        }
    }
}
