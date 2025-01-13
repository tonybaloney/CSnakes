using CSnakes.Runtime.EnvironmentManagement;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.Runtime.PackageManagement;

/// <summary>
/// Represents an interface for installing Python packages.
/// </summary>
public interface IPythonPackageInstaller
{
    /// <summary>
    /// Installs the specified packages.
    /// </summary>
    /// <param name="home">The home directory.</param>
    /// <param name="virtualEnvironmentLocation">The location of the virtual environment (optional).</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    Task InstallPackages(string home, IEnvironmentManagement? environmentManager);

    public static void ExecuteProcess(string fileName, string arguments, string workingDirectory, string path, ILogger logger, string? virtualEnv = null)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = workingDirectory,
            FileName = fileName,
            Arguments = arguments
        };

        if (!string.IsNullOrEmpty(path))
            startInfo.EnvironmentVariables["PATH"] = path;
        if (!string.IsNullOrEmpty(virtualEnv))
            startInfo.EnvironmentVariables["VIRTUAL_ENV"] = virtualEnv;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        logger.LogDebug($"Running {startInfo.FileName} with args {startInfo.Arguments} from {startInfo.WorkingDirectory}");

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
