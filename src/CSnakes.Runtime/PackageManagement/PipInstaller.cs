using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.PackageManagement;
internal class PipInstaller(ILogger<PipInstaller> logger) : IPythonPackageInstaller
{
    static readonly string pipBinaryName = $"pip{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    public Task InstallPackages(string home, string? virtualEnvironmentLocation)
    {
        string requirementsPath = Path.Combine(home, "requirements.txt");
        if (File.Exists(requirementsPath))
        {
            logger.LogInformation("File {Requirements} was found.", requirementsPath);
            InstallPackagesWithPip(home, virtualEnvironmentLocation);
        }
        else
        {
            logger.LogWarning("File {Requirements} was not found.", requirementsPath);
        }

        return Task.CompletedTask;
    }

    private static void InstallPackagesWithPip(string home, string? virtualEnvironmentLocation)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = home,
            FileName = pipBinaryName,
            Arguments = "install -r requirements.txt"
        };

        if (virtualEnvironmentLocation is not null)
        {
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            startInfo.FileName = Path.Combine(venvScriptPath, pipBinaryName);
            startInfo.EnvironmentVariables["PATH"] = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
        }

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using Process process = new() { StartInfo = startInfo };
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Failed to install packages.");
        }
    }
}
