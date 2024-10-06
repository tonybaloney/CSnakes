using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.Runtime.EnvironmentManagement;
internal class VenvEnvironmentManagement : IEnvironmentManagement
{
    private readonly string path;
    private readonly bool ensureExists;

    internal VenvEnvironmentManagement(string path, bool ensureExists)
    {
        this.path = path;
        this.ensureExists = ensureExists;
    }

    public void EnsureEnvironment(ILogger logger, PythonLocationMetadata pythonLocation)
    {
        if (!ensureExists)
            return;

        if (string.IsNullOrEmpty(path))
        {
            logger.LogError("Virtual environment location is not set but it was requested to be created.");
            throw new ArgumentNullException(nameof(path), "Virtual environment location is not set.");
        }
        var fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(path))
        {
            logger.LogInformation("Creating virtual environment at {VirtualEnvPath} using {PythonBinaryPath}", fullPath, pythonLocation.PythonBinaryPath);
            using Process process1 = ProcessUtils.ExecutePythonCommand(logger, pythonLocation, $"-VV");
            using Process process2 = ProcessUtils.ExecutePythonCommand(logger, pythonLocation, $"-m venv {fullPath}");
        }
        else
        {
            logger.LogDebug("Virtual environment already exists at {VirtualEnvPath}", fullPath);
        }
    }

    public string GetPath()
    {
        return path;
    }
}
