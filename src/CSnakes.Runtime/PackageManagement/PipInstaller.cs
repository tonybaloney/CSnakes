using CSnakes.Runtime.EnvironmentManagement;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.PackageManagement;

internal class PipInstaller(ILogger<PipInstaller> logger, string requirementsFileName) : IPythonPackageInstaller
{
    static readonly string pipBinaryName = $"pip{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    public Task InstallPackages(string home, IEnvironmentManagement? environmentManager)
    {
        // TODO:Allow overriding of the requirements file name.
        string requirementsPath = Path.GetFullPath(Path.Combine(home, requirementsFileName));
        if (File.Exists(requirementsPath))
        {
            logger.LogDebug("File {Requirements} was found.", requirementsPath);
            InstallPackagesWithPip(home, environmentManager);
        }
        else
        {
            logger.LogWarning("File {Requirements} was not found.", requirementsPath);
        }

        return Task.CompletedTask;
    }

    private void InstallPackagesWithPip(string home, IEnvironmentManagement? environmentManager)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = home,
            FileName = pipBinaryName,
            Arguments = $"install -r {requirementsFileName} --disable-pip-version-check"
        };

        if (environmentManager is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentManager.GetPath());
            logger.LogDebug("Using virtual environment at {VirtualEnvironmentLocation} to install packages with pip.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            // TODO: Check that the pip executable exists, and if not, raise an exception with actionable steps.
            startInfo.FileName = Path.Combine(venvScriptPath, pipBinaryName);
            startInfo.EnvironmentVariables["PATH"] = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
        }

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using Process process = new() { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                logger.LogDebug("{Data}", e.Data);
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
